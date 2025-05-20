/*******************************************************************************
 *     SDR Trunk 
 *     Copyright (C) 2014 Dennis Sheirer
 *
 *     This program is free software: you can redistribute it and/or modify
 *     it under the terms of the GNU General Public License as published by
 *     the Free Software Foundation, either version 3 of the License, or
 *     (at your option) any later version.
 *
 *     This program is distributed in the hope that it will be useful,
 *     but WITHOUT ANY WARRANTY; without even the implied warranty of
 *     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *     GNU General Public License for more details.
 *
 *     You should have received a copy of the GNU General Public License
 *     along with this program.  If not, see <http://www.gnu.org/licenses/>
 ******************************************************************************/
package io.github.dsheirer.module.log;

import io.github.dsheirer.message.IMessage;
import io.github.dsheirer.message.IMessageListener;
import io.github.dsheirer.sample.Listener;
import io.github.dsheirer.util.TimeStamp;
import java.net.DatagramPacket;
import java.net.DatagramSocket;
import java.net.InetAddress;
import java.nio.file.Path;

public class MessageEventLogger extends EventLogger implements IMessageListener, Listener<IMessage>
{
    public enum Type
    {
        BINARY, DECODED
    }

    private Type mType;

    public MessageEventLogger(Path logDirectory, String fileNameSuffix, Type type, long frequency)
    {
        super(logDirectory, fileNameSuffix, frequency);
        mType = type;
        hashValue = Integer.toString(fileNameSuffix.hashCode()) + " ";
    }

    @Override
    public Listener<IMessage> getMessageListener()
    {
        return this;
    }

    @Override
    public void reset()
    {
    }

    private String hashValue = "";

    @Override
    public void receive(IMessage message)
    {
        StringBuilder sb = new StringBuilder();
        sb.append(TimeStamp.getTimeStamp(message.getTimestamp(), " "));
        sb.append(",");
        sb.append((message.isValid() ? "PASSED" : "FAILED"));
        sb.append(",");
        sb.append(message.toString());

        write(sb.toString());

        //Customized to send important log information as an UDP packet to be processed by another application on same machine.
        String msg = sb.toString();
        if(msg.indexOf("SYNC LOSS") > 0  ||  (message.isValid() && (
                msg.indexOf("NET_STATUS_BCAST") > 0 ||
                        msg.indexOf("RFSS_STATUS_BCST") > 0 ||
                        msg.indexOf("ACK_RESPONSE_FNE") > 0 ||
                        msg.indexOf("DE_REGIST_ACK") > 0 ||
                        msg.indexOf("GRP_AFFIL_RESP") > 0 ||
                        msg.indexOf("GRP_VCH_GRANT") > 0 ||
                        msg.indexOf("GRP_VCH_GRNT_UPD") > 0 ||
                        msg.indexOf("LOCN_REG_RESPONS") > 0 ||
                        msg.indexOf("SNDCP_DCH_GRANT") > 0 ||
                        msg.indexOf("UNIT_REG_RESPONS") > 0 ))) {

            try {
                DatagramSocket clientSocket = new DatagramSocket();
                InetAddress address = InetAddress.getByName("localhost");
                byte[] sendData = (hashValue + message.toString()).getBytes();
                DatagramPacket sendPacket = new DatagramPacket(sendData, sendData.length, address, 7378);
                clientSocket.send(sendPacket);
            } catch (Exception e) {

            }
        }
    }

    @Override
    public String getHeader()
    {
        return mType.toString() + " Message Logger\n";
    }
}
