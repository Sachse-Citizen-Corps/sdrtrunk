/*
 * *****************************************************************************
 * Copyright (C) 2014-2024 Dennis Sheirer
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>
 * ****************************************************************************
 */
package io.github.dsheirer.audio.broadcast.mqtt;

import com.google.gson.Gson;
import com.google.gson.GsonBuilder;
import io.github.dsheirer.alias.Alias;
import io.github.dsheirer.channel.metadata.ChannelMetadata;
import io.github.dsheirer.channel.metadata.ChannelMetadataField;
import io.github.dsheirer.channel.metadata.IChannelMetadataUpdateListener;
import io.github.dsheirer.identifier.Identifier;
import org.eclipse.paho.client.mqttv3.IMqttDeliveryToken;
import org.eclipse.paho.client.mqttv3.MqttCallback;
import org.eclipse.paho.client.mqttv3.MqttClient;
import org.eclipse.paho.client.mqttv3.MqttConnectOptions;
import org.eclipse.paho.client.mqttv3.MqttException;
import org.eclipse.paho.client.mqttv3.MqttMessage;
import org.eclipse.paho.client.mqttv3.persist.MemoryPersistence;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import java.util.HashMap;
import java.util.List;
import java.util.Map;
import java.util.concurrent.ConcurrentHashMap;
import java.util.concurrent.atomic.AtomicBoolean;
import java.util.stream.Collectors;

/**
 * MQTT publisher for Now Playing metadata updates.
 * Listens to channel metadata changes and publishes them to an MQTT broker.
 */
public class MQTTNowPlayingPublisher implements IChannelMetadataUpdateListener, MqttCallback
{
    private final static Logger mLog = LoggerFactory.getLogger(MQTTNowPlayingPublisher.class);

    private MQTTConfiguration mConfiguration;
    private MqttClient mMqttClient;
    private Gson mGson;
    private AtomicBoolean mConnected = new AtomicBoolean(false);
    private Map<ChannelMetadata, Map<String, Object>> mChannelDataCache = new ConcurrentHashMap<>();

    /**
     * Constructs a new MQTT Now Playing publisher
     * @param configuration MQTT configuration
     */
    public MQTTNowPlayingPublisher(MQTTConfiguration configuration)
    {
        mConfiguration = configuration;
        mGson = new GsonBuilder().setPrettyPrinting().create();
    }

    /**
     * Starts the MQTT publisher by connecting to the broker
     */
    public void start()
    {
        if (!mConfiguration.isValid())
        {
            mLog.error("Cannot start MQTT publisher - invalid configuration: " + mConfiguration);
            return;
        }

        try
        {
            String brokerUrl = mConfiguration.getBrokerUrl();
            String clientId = mConfiguration.getClientId();

            mLog.info("Connecting to MQTT broker: " + brokerUrl + " with client ID: " + clientId);

            mMqttClient = new MqttClient(brokerUrl, clientId, new MemoryPersistence());
            mMqttClient.setCallback(this);

            MqttConnectOptions options = new MqttConnectOptions();
            options.setCleanSession(true);
            options.setAutomaticReconnect(true);

            if (mConfiguration.hasUsername())
            {
                options.setUserName(mConfiguration.getUsername());
            }

            if (mConfiguration.hasPassword())
            {
                options.setPassword(mConfiguration.getPassword().toCharArray());
            }

            mMqttClient.connect(options);
            mConnected.set(true);

            mLog.info("Successfully connected to MQTT broker: " + brokerUrl);
        }
        catch (MqttException e)
        {
            mLog.error("Error connecting to MQTT broker", e);
            mConnected.set(false);
        }
    }

    /**
     * Stops the MQTT publisher and disconnects from the broker
     */
    public void stop()
    {
        if (mMqttClient != null && mMqttClient.isConnected())
        {
            try
            {
                // Send a final update clearing all channels
                publishClearMessage();

                mMqttClient.disconnect();
                mMqttClient.close();
                mLog.info("Disconnected from MQTT broker");
            }
            catch (MqttException e)
            {
                mLog.error("Error disconnecting from MQTT broker", e);
            }
            finally
            {
                mConnected.set(false);
                mChannelDataCache.clear();
            }
        }
    }

    /**
     * Indicates if the publisher is currently connected to the MQTT broker
     */
    public boolean isConnected()
    {
        return mConnected.get() && mMqttClient != null && mMqttClient.isConnected();
    }

    @Override
    public void updated(ChannelMetadata metadata, ChannelMetadataField field)
    {
        if (!isConnected())
        {
            return;
        }

        try
        {
            Map<String, Object> channelData = convertToMap(metadata);
            mChannelDataCache.put(metadata, channelData);

            // Publish all active channels
            publishAllChannels();
        }
        catch (Exception e)
        {
            mLog.error("Error processing metadata update", e);
        }
    }

    /**
     * Removes a channel from tracking when it becomes inactive
     */
    public void removeChannel(ChannelMetadata metadata)
    {
        mChannelDataCache.remove(metadata);
        publishAllChannels();
    }

    /**
     * Publishes all currently active channels to MQTT
     */
    private void publishAllChannels()
    {
        if (!isConnected())
        {
            return;
        }

        try
        {
            Map<String, Object> payload = new HashMap<>();
            payload.put("timestamp", System.currentTimeMillis());
            payload.put("channels", mChannelDataCache.values());

            String json = mGson.toJson(payload);
            MqttMessage message = new MqttMessage(json.getBytes());
            message.setQos(mConfiguration.getQos());
            message.setRetained(false);

            mMqttClient.publish(mConfiguration.getTopic(), message);

            mLog.debug("Published " + mChannelDataCache.size() + " channels to MQTT topic: " +
                      mConfiguration.getTopic());
        }
        catch (MqttException e)
        {
            mLog.error("Error publishing to MQTT", e);
        }
    }

    /**
     * Publishes a message indicating no channels are active
     */
    private void publishClearMessage()
    {
        try
        {
            Map<String, Object> payload = new HashMap<>();
            payload.put("timestamp", System.currentTimeMillis());
            payload.put("channels", List.of());

            String json = mGson.toJson(payload);
            MqttMessage message = new MqttMessage(json.getBytes());
            message.setQos(mConfiguration.getQos());
            message.setRetained(false);

            mMqttClient.publish(mConfiguration.getTopic(), message);
        }
        catch (MqttException e)
        {
            mLog.error("Error publishing clear message to MQTT", e);
        }
    }

    /**
     * Converts ChannelMetadata to a Map for JSON serialization
     */
    private Map<String, Object> convertToMap(ChannelMetadata metadata)
    {
        Map<String, Object> map = new HashMap<>();

        // Add basic channel information
        if (metadata.hasTimeslot())
        {
            map.put("timeslot", metadata.getTimeslot());
        }

        // Add decoder information
        if (metadata.hasDecoderTypeIdentifier())
        {
            map.put("decoder_type", toString(metadata.getDecoderTypeConfigurationIdentifier()));
        }

        if (metadata.hasDecoderStateIdentifier())
        {
            map.put("state", toString(metadata.getChannelStateIdentifier()));
        }

        // Add system/site/channel configuration
        if (metadata.hasSystemConfigurationIdentifier())
        {
            map.put("system", toString(metadata.getSystemConfigurationIdentifier()));
        }

        if (metadata.hasSiteConfigurationIdentifier())
        {
            map.put("site", toString(metadata.getSiteConfigurationIdentifier()));
        }

        if (metadata.hasChannelConfigurationIdentifier())
        {
            map.put("channel", toString(metadata.getChannelNameConfigurationIdentifier()));
        }

        if (metadata.hasDecoderLogicalChannelNameIdentifier())
        {
            map.put("logical_channel", toString(metadata.getDecoderLogicalChannelNameIdentifier()));
        }

        // Add frequency
        if (metadata.hasFrequencyConfigurationIdentifier())
        {
            map.put("frequency", toString(metadata.getFrequencyConfigurationIdentifier()));
        }

        // Add FROM user information
        if (metadata.hasFromIdentifier())
        {
            Map<String, Object> fromData = new HashMap<>();
            fromData.put("identifier", toString(metadata.getFromIdentifier()));

            List<Alias> fromAliases = metadata.getFromIdentifierAliases();
            if (fromAliases != null && !fromAliases.isEmpty())
            {
                fromData.put("aliases", fromAliases.stream()
                    .map(Alias::getName)
                    .collect(Collectors.toList()));
            }

            if (metadata.hasTalkerAliasIdentifier())
            {
                fromData.put("talker_alias", toString(metadata.getTalkerAliasIdentifier()));
            }

            map.put("from", fromData);
        }

        // Add TO user information
        if (metadata.hasToIdentifier())
        {
            Map<String, Object> toData = new HashMap<>();
            toData.put("identifier", toString(metadata.getToIdentifier()));

            List<Alias> toAliases = metadata.getToIdentifierAliases();
            if (toAliases != null && !toAliases.isEmpty())
            {
                toData.put("aliases", toAliases.stream()
                    .map(Alias::getName)
                    .collect(Collectors.toList()));
            }

            map.put("to", toData);
        }

        return map;
    }

    /**
     * Safely converts an identifier to string
     */
    private String toString(Identifier identifier)
    {
        return identifier != null ? identifier.toString() : null;
    }

    // MqttCallback interface methods

    @Override
    public void connectionLost(Throwable cause)
    {
        mLog.warn("MQTT connection lost", cause);
        mConnected.set(false);
    }

    @Override
    public void messageArrived(String topic, MqttMessage message)
    {
        // Not used - we only publish, not subscribe
    }

    @Override
    public void deliveryComplete(IMqttDeliveryToken token)
    {
        // Optionally log successful delivery
    }
}
