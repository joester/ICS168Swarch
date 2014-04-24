using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;

namespace SwarchServer
{
    static class NTPTime
    {
                    /*
                    This function will query an NTP server stated as the URL or IP address.
                    I call this function only once at the beginning when I initialize the client
        code and when I initialize the server.
                    This function is static so you don't have to instantiate the class. It will
        require a C# timer
                    in order to start the timer from the time that you receive the time from the
        NTP server. So as your
                    game is running, in order for you to return and set a time, or make a timestamp
        for that matter, add the
                    elapsed time of the timer to the NTP time that you received.
            
                    So, to call this function successfully from your game, I make a universal timer
        to my program called
                    uniClock and a universal DateTime object and I call
                    dt = getNTPTime ( uniClock );
                    This will set my DateTime object in my game and start the clock.
                    Any time throughout my game that I wish to make a TimeStamp, I simply call
                    ((dt.AddMinutes(uniClock.Elapsed.Minutes).AddSeconds(uniClock.Elapsed.Seconds).AddMilliseconds(uniClock.Elapsed.Milliseconds)).Ticks))
                    In essence, it is just emulating the computer time, which will be sufficient
        for the short duration of our game.
            
                    For this NTP time to work, ALL THE CLIENTS AND THE SERVER MUST USE THIS
        FUNCTION AND MUST RETRIEVE THE TIME FROM THE SAME TIME SERVER.        
            
                    Pay attention to the way that the network latency is being calculated and added
        to the time.
                    */
                public static DateTime getNTPTime( ref Stopwatch uniClock )
                {
                    Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram,ProtocolType.Udp);

                    //IPAddress serverAddr = IPAddress.Parse("nist1-la.ustiming.org ");

                    IPAddress serverAddr = IPAddress.Parse("64.147.116.229");

                    IPEndPoint endPoint = new IPEndPoint(serverAddr, 123);

                    byte[] ntpData = new byte[48];

                    ntpData[0] = 0x1B;
                    ulong currSeconds = (ulong)((DateTime.UtcNow - new DateTime(1900, 1,1)).TotalSeconds);
                    ulong currMillisecs = (ulong)((DateTime.UtcNow - new DateTime(1900, 1, 1)).TotalMilliseconds);
                    ulong tempSeconds;
                    ntpData[40] = (byte)(currSeconds >> 24);
                    tempSeconds = currSeconds << 40;
                    ntpData[41] = (byte)(tempSeconds >> 56);
                    tempSeconds = currSeconds << 48;
                    ntpData[42] = (byte)(tempSeconds >> 56);
                    tempSeconds = currSeconds << 56;
                    ntpData[43] = (byte)(tempSeconds >> 56);

                    currMillisecs = (ulong)(((UInt32)(currMillisecs / 1000)) * UInt32.MaxValue);

                    ntpData[44] = (byte)(currMillisecs >> 24);
                    tempSeconds = currMillisecs << 40;
                    ntpData[45] = (byte)(tempSeconds >> 56);
                    tempSeconds = currMillisecs << 48;
                    ntpData[46] = (byte)(tempSeconds >> 56);
                    tempSeconds = currMillisecs << 56;
                    ntpData[47] = (byte)(tempSeconds >> 56);


                    sock.Connect(endPoint);
                    DateTime T1 = DateTime.UtcNow;
                    //Console.WriteLine("T1 : = " + T1 + " " + T1.Millisecond);
                    sock.Send(ntpData);
                    while (sock.Receive(ntpData) < 44) { Console.WriteLine("getting NTP"); }
                    DateTime T4 = DateTime.UtcNow;
                    //Console.WriteLine("T4 : = " + T4 + " " + T4.Millisecond);
                    //UInt32 destTime = (UInt32)(ntpData[16] << 24) | (UInt32)(ntpData[17]<< 16) | (UInt32)(ntpData[18] << 8) | (UInt32)(ntpData[19]);
                    sock.Close();

                    //Console.WriteLine("LI (lead indicator) : " + (ntpData[0] >> 6));
                    int temp = ntpData[0] << 2;
                    temp = temp >> 5;
                    //Console.WriteLine("VN (version number) : " + temp);
                    temp = (byte)(ntpData[0] << 5);
                    temp = temp >> 5;
                    //Console.WriteLine("Mode : " + temp);
                    //Console.WriteLine("Stratum Level : " + ntpData[1]);
                    //Console.WriteLine("Poll Interval :  " + ntpData[2]);
                    //Console.WriteLine("Precision : " + ntpData[3]);

                                /*
                                These are the 4 time stamps that are retrieved from the NTP Packet.
                                More on the details of these packets can be researched from the NTP documentation.
                        
                                Reference Timestamp: This is the local time at which the local clock was last set
        or corrected, in
                                64-bit timestamp format.

                                Originate Timestamp: This is the local time at which the request departed the
        client host for the
                                service host, in 64-bit timestamp format.
                        
                                Receive Timestamp: This is the local time at which the request arrived at the
        service host, in 64-bit
                                timestamp format.
                        
                                Transmit Timestamp: This is the local time at which the reply departed the
        service host for the client
                                host, in 64-bit timestamp format.
                        
                                */
                    TimeSpan calculationTime;
                    UInt32 refTime = (UInt32)(ntpData[16] << 24) | (UInt32)(ntpData[17] <<
        16) | (UInt32)(ntpData[18] << 8) | (UInt32)(ntpData[19]);
                    UInt32 refTimeMicro = (UInt32)(ntpData[20] << 24) | (UInt32)(ntpData[21]
        << 16) | (UInt32)(ntpData[22] << 8) | (UInt32)(ntpData[23]);
                    UInt32 refTimemilliseconds = (UInt32)(((double)refTimeMicro /
        UInt32.MaxValue) * 1000);

                    UInt32 origTime = (UInt32)(ntpData[24] << 24) | (UInt32)(ntpData[25] <<
        16) | (UInt32)(ntpData[26] << 8) | (UInt32)(ntpData[27]);
                    UInt32 origTimeMicro = (UInt32)(ntpData[28] << 24) |
        (UInt32)(ntpData[29] << 16) | (UInt32)(ntpData[30] << 8) |
        (UInt32)(ntpData[31]);
                    UInt32 origTimemilliseconds = (UInt32)(((double)origTimeMicro /
        UInt32.MaxValue) * 1000);

                    UInt32 recTime = (UInt32)(ntpData[32] << 24) | (UInt32)(ntpData[33] <<
        16) | (UInt32)(ntpData[34] << 8) | (UInt32)(ntpData[35]);
                    UInt32 recTimeMicro = (UInt32)(ntpData[36] << 24) | (UInt32)(ntpData[37]
        << 16) | (UInt32)(ntpData[38] << 8) | (UInt32)(ntpData[39]);
                    UInt32 recTimemilliseconds = (UInt32)(((double)recTimeMicro /
        UInt32.MaxValue) * 1000);

                    UInt32 transTime = (UInt32)(ntpData[40] << 24) | (UInt32)(ntpData[41] <<
        16) | (UInt32)(ntpData[42] << 8) | (UInt32)(ntpData[43]);
                    UInt32 transTimeMicro = (UInt32)(ntpData[44] << 24) |
        (UInt32)(ntpData[45] << 16) | (UInt32)(ntpData[46] << 8) |
        (UInt32)(ntpData[47]);
                    UInt32 milliseconds = (UInt32)(((double)transTimeMicro /
        UInt32.MaxValue) * 1000);


                    DateTime BaseDateExample = new DateTime(1900, 1, 1, 0, 0, 0, 0,
        DateTimeKind.Utc);
                    Console.WriteLine("Reference time stamp : " +
        BaseDateExample.AddSeconds(refTime).AddMilliseconds(refTimemilliseconds));
                    Console.WriteLine("Originate time stamp : " +
        BaseDateExample.AddSeconds(origTime).AddMilliseconds(origTimemilliseconds));


                    Console.WriteLine("Receive time stamp   : " +
        BaseDateExample.AddSeconds(recTime).AddMilliseconds(recTimemilliseconds)
        + " " +
        (BaseDateExample.AddSeconds(recTime).AddMilliseconds(recTimemilliseconds)).Millisecond);
                    Console.WriteLine("Transmit time stamp  : " +
        BaseDateExample.AddSeconds(transTime).AddMilliseconds(milliseconds) + " " +
        (BaseDateExample.AddSeconds(transTime).AddMilliseconds(milliseconds)).Millisecond);

                    DateTime BaseDate = new DateTime(1900, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                    DateTime dt = BaseDate.AddSeconds(transTime).AddMilliseconds(milliseconds);


                    DateTime BaseDate1 = new DateTime(1900, 1, 1, 0, 0, 0, 0,
        DateTimeKind.Utc);
                    DateTime T2 =
        BaseDate1.AddSeconds(recTime).AddMilliseconds(recTimemilliseconds);

                    calculationTime = dt.Subtract(T2);


                    TimeSpan delay = T4.Subtract(T1);
                    double networkDelay = delay.Milliseconds / 2.0;

                    //Console.WriteLine( "Network Latency to NTP Server : " + (delay / 2.0)) ;


                    //tsOffset = tsOffset.Add(dt.Subtract(T4));

                    dt = dt.AddMilliseconds(networkDelay);

            


                    uniClock.Start();

                    return dt;
            
        }

    }
}
