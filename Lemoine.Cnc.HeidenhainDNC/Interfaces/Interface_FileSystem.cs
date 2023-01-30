// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: GPL-2.0-or-later

using System;
using System.Runtime.InteropServices;

namespace Lemoine.Cnc
{
  /// <summary>
  /// Description of Interface_fileSystem.
  /// </summary>
  public class Interface_fileSystem: GenericInterface<HeidenhainDNCLib.IJHFileSystem>
  {
    #region Members
    #endregion // Members

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public Interface_fileSystem () : base(HeidenhainDNCLib.DNC_INTERFACE_OBJECT.DNC_INTERFACE_JHFILESYSTEM) {}
    #endregion // Constructors

    #region Protected methods
    /// <summary>
    /// Extra work during the initialization
    /// Already within a try catch
    /// </summary>
    protected override void InitializeInstance(InterfaceParameters parameters)
    {
      // Nothing
    }
    
    /// <summary>
    /// Work to do on "start", before new data will be read
    /// Already within a try catch
    /// </summary>
    protected override void ResetDataInstance()
    {

    }
    #endregion // Protected methods

    #region Get methods

    /// <summary>
    /// Current program that is read
    /// </summary>
    public void GetProgramFirstLines (string fileName, string localFileName) 
    {
      m_interface.ReceiveFile (fileName, localFileName);
    }

    /// <summary>
    /// Get current directory
    /// </summary>
    public string GetCurrentDirectory ()
    {
      return m_interface.GetCurrentDirectory ();
    }

    /// <summary>
    /// Change current directory
    /// </summary>
    public void ChangeDirectory (string directory)
    {
      m_interface.ChangeDirectory (directory);
    }

    /// <summary>
    /// Receive a remote file to local file
    /// </summary>
    public void ReceiveFile (string fileName, string localFileName)
    {
      m_interface.ReceiveFile (fileName, localFileName);
    }

    /// <summary>
    /// Delete a remote file
    /// </summary>
    public void DeleteFile (string fileName)
    {
      m_interface.DeleteFile (fileName);
    }

    /// <summary>
    /// Read content of current directory
    /// </summary>
    public HeidenhainDNCLib.IJHDirectoryEntryList ReadDirectory (string bstrFileName, HeidenhainDNCLib.JHFileAttributes pAttributeSelection, HeidenhainDNCLib.JHFileAttributes pAttributeState)
    {
      return m_interface.ReadDirectory (bstrFileName, pAttributeSelection, pAttributeState);
    }

    #endregion // Get methods

    #region Private methods

    #endregion // Private methods
  }
}
