/*
 * *****************************************************************************
 *  Copyright (C) 2014-2020 Dennis Sheirer
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

package io.github.dsheirer.module.decode.dmr.message.data.block;

import io.github.dsheirer.bits.CorrectedBinaryMessage;
import io.github.dsheirer.module.decode.dmr.message.CACH;
import io.github.dsheirer.module.decode.dmr.message.data.SlotType;
import io.github.dsheirer.module.decode.dmr.sync.DMRSyncPattern;

/**
 * Unknown DMR data block
 */
public class UnknownDataBlock extends DataBlock
{
    /**
     * Constructs an instance.
     *
     * @param syncPattern either BASE_STATION_DATA or MOBILE_STATION_DATA
     * @param message containing extracted 196-bit payload.
     * @param cach for the DMR burst
     * @param slotType for this data message
     * @param timestamp message was received
     * @param timeslot for the DMR burst
     */
    public UnknownDataBlock(DMRSyncPattern syncPattern, CorrectedBinaryMessage message, CACH cach,
                            SlotType slotType, long timestamp, int timeslot)
    {
        super(syncPattern, message, cach, slotType, timestamp, timeslot);
    }

    @Override
    public CorrectedBinaryMessage getConfirmedPayload()
    {
        return getMessage();
    }

    @Override
    public CorrectedBinaryMessage getUnConfirmedPayload()
    {
        return getMessage();
    }

    @Override
    public int getDataBlockSerialNumber()
    {
        return 0;
    }
}
