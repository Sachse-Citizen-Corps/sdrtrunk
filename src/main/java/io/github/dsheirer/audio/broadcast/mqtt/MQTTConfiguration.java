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

import com.fasterxml.jackson.annotation.JsonIgnore;
import com.fasterxml.jackson.dataformat.xml.annotation.JacksonXmlProperty;
import io.github.dsheirer.audio.broadcast.BroadcastConfiguration;
import io.github.dsheirer.audio.broadcast.BroadcastFormat;
import io.github.dsheirer.audio.broadcast.BroadcastServerType;
import javafx.beans.property.IntegerProperty;
import javafx.beans.property.SimpleIntegerProperty;
import javafx.beans.property.SimpleStringProperty;
import javafx.beans.property.StringProperty;

/**
 * MQTT configuration for publishing Now Playing metadata updates
 */
public class MQTTConfiguration extends BroadcastConfiguration
{
    private static final int DEFAULT_QOS = 0;
    private static final String DEFAULT_TOPIC = "sdrtrunk/nowplaying";
    private static final String DEFAULT_CLIENT_ID = "sdrtrunk";

    private StringProperty mTopic = new SimpleStringProperty(DEFAULT_TOPIC);
    private IntegerProperty mQos = new SimpleIntegerProperty(DEFAULT_QOS);
    private StringProperty mClientId = new SimpleStringProperty(DEFAULT_CLIENT_ID);
    private StringProperty mUsername = new SimpleStringProperty();

    /**
     * Constructs an MQTT configuration
     */
    public MQTTConfiguration()
    {
        super(BroadcastFormat.MP3); // Format is not used for metadata-only publishing
        setPort(1883); // Default MQTT port
    }

    @Override
    public BroadcastConfiguration copyOf()
    {
        MQTTConfiguration copy = new MQTTConfiguration();
        copy.setName(getName());
        copy.setHost(getHost());
        copy.setPort(getPort());
        copy.setUsername(getUsername());
        copy.setPassword(getPassword());
        copy.setTopic(getTopic());
        copy.setQos(getQos());
        copy.setClientId(getClientId());
        copy.setEnabled(isEnabled());
        return copy;
    }

    @Override
    public BroadcastServerType getBroadcastServerType()
    {
        return BroadcastServerType.MQTT_NOW_PLAYING;
    }

    /**
     * MQTT topic to publish Now Playing updates to
     */
    @JacksonXmlProperty(isAttribute = true, localName = "topic")
    public String getTopic()
    {
        return mTopic.get();
    }

    public void setTopic(String topic)
    {
        mTopic.set(topic);
    }

    public StringProperty topicProperty()
    {
        return mTopic;
    }

    public boolean hasTopic()
    {
        return mTopic.get() != null && !mTopic.get().isEmpty();
    }

    /**
     * MQTT Quality of Service level (0, 1, or 2)
     */
    @JacksonXmlProperty(isAttribute = true, localName = "qos")
    public int getQos()
    {
        return mQos.get();
    }

    public void setQos(int qos)
    {
        if (qos >= 0 && qos <= 2)
        {
            mQos.set(qos);
        }
    }

    public IntegerProperty qosProperty()
    {
        return mQos;
    }

    /**
     * MQTT Client ID
     */
    @JacksonXmlProperty(isAttribute = true, localName = "client_id")
    public String getClientId()
    {
        return mClientId.get();
    }

    public void setClientId(String clientId)
    {
        mClientId.set(clientId);
    }

    public StringProperty clientIdProperty()
    {
        return mClientId;
    }

    public boolean hasClientId()
    {
        return mClientId.get() != null && !mClientId.get().isEmpty();
    }

    /**
     * MQTT username for authentication (optional)
     */
    @JacksonXmlProperty(isAttribute = true, localName = "username")
    public String getUsername()
    {
        return mUsername.get();
    }

    public void setUsername(String username)
    {
        mUsername.set(username);
    }

    public StringProperty usernameProperty()
    {
        return mUsername;
    }

    public boolean hasUsername()
    {
        return mUsername.get() != null && !mUsername.get().isEmpty();
    }

    /**
     * Broker URL in the format tcp://host:port
     */
    @JsonIgnore
    public String getBrokerUrl()
    {
        return "tcp://" + getHost() + ":" + getPort();
    }

    /**
     * Validates the MQTT configuration
     */
    @Override
    @JsonIgnore
    public boolean isValid()
    {
        return hasHost() && hasPort() && hasTopic() && hasClientId();
    }

    @Override
    public String toString()
    {
        return "MQTT [" + getBrokerUrl() + " Topic:" + getTopic() + "]";
    }
}
