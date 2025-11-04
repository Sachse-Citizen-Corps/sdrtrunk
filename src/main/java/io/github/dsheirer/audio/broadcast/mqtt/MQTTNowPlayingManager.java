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

import io.github.dsheirer.channel.metadata.ChannelAndMetadata;
import io.github.dsheirer.channel.metadata.ChannelMetadata;
import io.github.dsheirer.channel.metadata.ChannelMetadataField;
import io.github.dsheirer.channel.metadata.IChannelMetadataUpdateListener;
import io.github.dsheirer.sample.Listener;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import java.util.ArrayList;
import java.util.List;
import java.util.Map;
import java.util.concurrent.ConcurrentHashMap;

/**
 * Manager for MQTT Now Playing publishers.
 * Coordinates multiple MQTT publishers and integrates with the channel metadata system.
 */
public class MQTTNowPlayingManager implements IChannelMetadataUpdateListener, Listener<ChannelAndMetadata>
{
    private final static Logger mLog = LoggerFactory.getLogger(MQTTNowPlayingManager.class);

    private List<MQTTNowPlayingPublisher> mPublishers = new ArrayList<>();
    private Map<ChannelMetadata, Boolean> mTrackedMetadata = new ConcurrentHashMap<>();

    /**
     * Constructs a new MQTT Now Playing manager
     */
    public MQTTNowPlayingManager()
    {
    }

    /**
     * Adds an MQTT configuration and starts a publisher for it
     * @param configuration MQTT configuration
     */
    public void addConfiguration(MQTTConfiguration configuration)
    {
        if (configuration != null && configuration.isEnabled())
        {
            MQTTNowPlayingPublisher publisher = new MQTTNowPlayingPublisher(configuration);
            publisher.start();
            mPublishers.add(publisher);
            mLog.info("Started MQTT Now Playing publisher: " + configuration);
        }
    }

    /**
     * Adds multiple MQTT configurations
     * @param configurations list of MQTT configurations
     */
    public void addConfigurations(List<MQTTConfiguration> configurations)
    {
        for (MQTTConfiguration configuration : configurations)
        {
            addConfiguration(configuration);
        }
    }

    /**
     * Stops all MQTT publishers and clears configurations
     */
    public void stop()
    {
        mLog.info("Stopping MQTT Now Playing manager");

        for (MQTTNowPlayingPublisher publisher : mPublishers)
        {
            publisher.stop();
        }

        mPublishers.clear();
        mTrackedMetadata.clear();
    }

    /**
     * Indicates if any publishers are currently active
     */
    public boolean hasActivePublishers()
    {
        return !mPublishers.isEmpty();
    }

    /**
     * Called when a channel is added to the metadata model
     * Implements Listener<ChannelAndMetadata> interface
     */
    @Override
    public void receive(ChannelAndMetadata channelAndMetadata)
    {
        if (mPublishers.isEmpty())
        {
            return;
        }

        // Register ourselves as a listener for each channel metadata
        for (ChannelMetadata metadata : channelAndMetadata.getChannelMetadata())
        {
            metadata.addUpdateEventListener(this);
            mTrackedMetadata.put(metadata, Boolean.TRUE);
            mLog.debug("Started tracking metadata for channel: " + channelAndMetadata.getChannel().getName());
        }
    }

    /**
     * Called when channel metadata is updated
     * Implements IChannelMetadataUpdateListener interface
     */
    @Override
    public void updated(ChannelMetadata metadata, ChannelMetadataField field)
    {
        if (mPublishers.isEmpty())
        {
            return;
        }

        // Forward the update to all active publishers
        for (MQTTNowPlayingPublisher publisher : mPublishers)
        {
            publisher.updated(metadata, field);
        }
    }

    /**
     * Called when a channel is removed from the metadata model
     * @param metadata channel metadata that was removed
     */
    public void removeChannel(ChannelMetadata metadata)
    {
        if (mPublishers.isEmpty())
        {
            return;
        }

        mTrackedMetadata.remove(metadata);
        metadata.removeUpdateEventListener(this);

        // Notify all publishers that the channel was removed
        for (MQTTNowPlayingPublisher publisher : mPublishers)
        {
            publisher.removeChannel(metadata);
        }

        mLog.debug("Stopped tracking metadata for removed channel");
    }

    /**
     * Gets the count of active publishers
     */
    public int getPublisherCount()
    {
        return mPublishers.size();
    }

    /**
     * Gets the count of tracked channels
     */
    public int getTrackedChannelCount()
    {
        return mTrackedMetadata.size();
    }
}
