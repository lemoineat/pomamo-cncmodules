// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: GPL-2.0-or-later

using System;

namespace Lemoine.Cnc
{
  /// <summary>
  /// Description of Interface_communication.
  /// </summary>
  public class Interface_communication : GenericMitsubishiInterface
  {
    #region Protected methods
    /// <summary>
    /// Work to do on "start", before new data will be read
    /// Already within a try catch
    /// </summary>
    protected override void ResetDataInstance ()
    {
      // Nothing for now
    }
    #endregion // Protected methods

    #region Methods
    /// <summary>
    /// Connection to the machine
    /// </summary>
    /// <param name="hostAddress"></param>
    /// <param name="port"></param>
    /// <param name="ncCardNumber"></param>
    /// <param name="headNumber"></param>
    public void Open (string hostAddress, int port, int ncCardNumber, int headNumber)
    {
      // Possibly configure the port and host
      if (!string.IsNullOrEmpty (hostAddress)) {
        if ((int)SystemType < 4 || SystemType == Mitsubishi.MitsubishiSystemType.MELDAS_C70) {
          Logger.Warn ("Mitsubishi - SetTCPIPProtocol cannot be used for system type '" + SystemType + "', no need to specify a host or IP");
        }
        else {
          Logger.InfoFormat ("Mitsubishi - Configuring the connection with hostAddress={0} and port={1}", hostAddress, port);
          int result = CommunicationObject.SetTCPIPProtocol (hostAddress, port);
          if (result > 0) {
            throw new ErrorCodeException (result, "SetTCPIPProtocol");
          }
        }
      }

      // Prepare the hostname
      const int timeOut = 10; // unit: 100ms, 10 is thus 1s
      string hostName = hostAddress;
      if (hostName.Equals ("localhost", StringComparison.InvariantCultureIgnoreCase) ||
          SystemType == Mitsubishi.MitsubishiSystemType.MELDAS_C6_C64 ||
          SystemType == Mitsubishi.MitsubishiSystemType.MELDAS_700L ||
          SystemType == Mitsubishi.MitsubishiSystemType.MELDAS_700M) {
        hostName = "EZNC_LOCALHOST";
      }

      if (SystemType == Mitsubishi.MitsubishiSystemType.MELDAS_C70) {
        Logger.Warn ("Mitsubishi - According to the documentation, connecting a machine MELDAS_C70 would require to use the function SetMelsecProtocol first, " +
                    "taking not less than 29 arguments => to be implemented later");
        //CommunicationObject.SetMelsecProtocol(...);
      }

      // Connection
      Logger.InfoFormat ("Mitsubishi - Connecting the machine with SystemType={0}, ncCardNumber={1}, timeOut={2}, hostName={3}",
                        SystemType + " (" + ((int)SystemType) + ")", ncCardNumber, timeOut, hostName);
      bool useOfOpen2 = (int)SystemType > 4;
      int result2 = useOfOpen2 ?
        CommunicationObject.Open2 ((int)SystemType, ncCardNumber, timeOut, hostName) :
        CommunicationObject.Open ((int)SystemType, ncCardNumber, timeOut, hostName);
      if (result2 != 0) {
        throw new ErrorCodeException (result2, useOfOpen2 ? "Open2" : "Open");
      }

      Logger.InfoFormat ("Mitsubishi - Successfully connected to the machine");

      // Set the head
      if (headNumber != -1) {
        result2 = 0;
        try {
          if ((result2 = CommunicationObject.SetHead (headNumber)) != 0) {
            throw new ErrorCodeException (result2, "SetHead");
          }
        }
        catch (Exception e) {
          Logger.ErrorFormat ("Mitsubishi.Open - Couldn't set the head: " + e.Message);
        }
      }
    }
    #endregion // Methods
  }

  /// <summary>
  /// Description of Interface_communication_2.
  /// </summary>
  public class Interface_communication_2 : Interface_communication
  {

  }

  /// <summary>
  /// Description of Interface_communication_3.
  /// </summary>
  public class Interface_communication_3 : Interface_communication
  {

  }
}
