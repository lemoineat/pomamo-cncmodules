// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: GPL-2.0-or-later

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Lemoine.Cnc
{
  /// <summary>
  /// HeidenhainDNC input module
  /// </summary>
  public partial class HeidenhainDNC
  {
    static readonly string REMOVE_REMOTE_FILES_MAX_FILE_NUMBER_KEY = "HeidenhainDNC.RemoveRemoteFiles.MaxFileNumber";
    static readonly int REMOVE_REMOTE_FILES_MAX_FILE_NUMBER_DEFAULT = 100; // Switch to about 5 in the future ?

    readonly IDictionary<string, DateTime> m_cacheDateTime = new Dictionary<string, DateTime> (); // Directory path => date/time
    readonly IDictionary<string, string> m_cacheData = new Dictionary<string, string> ();

    #region Get methods
    /// <summary>
    /// string to identify part comment in program content
    /// 
    /// </summary>
    public string CommentSearchString { get; set; }

    /// <summary>
    /// string to check program file name
    /// 
    /// </summary>
    public string ProgramFileNamePattern { get; set; }

    /// <summary>
    /// string to check subprogram file name
    /// 
    /// </summary>
    public string SubProgramFileNamePattern { get; set; }

    /// <summary>
    /// string to check opid file name
    /// 
    /// </summary>
    public string OpIdFileNamePattern { get; set; }

    /// <summary>
    /// Get a double value from a stamp file
    /// </summary>
    /// <param name="param">list with separator: ;file pathname; main stamp file;stamp file prefix;stamp file separator;file read time interval </param>
    /// <returns></returns>
    [Obsolete ("Remove the implementation since there was some weakness in the implementation", true)]
    public double GetValueFromStampFile (string param)
    {
      var s = GetStringValueFromStampFile (param);
      try {
        return double.Parse (s, System.Globalization.CultureInfo.InvariantCulture);
      }
      catch (Exception ex) {
        log.Error ($"GetValueFromStampFile: {s} is not a valid double", ex);
        throw;
      }
    }

    /// <summary>
    /// get value from a stamp file.
    /// </summary>
    /// <param name="param">list with separator: ;file pathname; main stamp file;stamp file prefix;stamp file separator;file read time interval </param>
    /// <returns></returns>
    [Obsolete ("Remove the implementation since there was some weakness in the implementation", true)]
    public string GetStringValueFromStampFile (String param)
    {
      string result = null;
      if (log.IsDebugEnabled) {
        log.Debug ($"GetStringValueFromStampFile: param={param}");
      }

      if (string.IsNullOrEmpty (param)) {
        log.ErrorFormat ("GetStringValueFromStampFile: empty parameter");
      }
      else {
        var paramItems = Lemoine.Collections.EnumerableString.ParseListString (param);
        if (paramItems.Length != 5) {
          log.ErrorFormat ("GetStringValueFromStampFile: bad number of parameters");
        }
        else {
          if (!int.TryParse (paramItems[4], out var skipTime)) {
            log.ErrorFormat ("GetStringValueFromStampFile: invalid time parameters");
          }
          else {
            result = GetStringValueFromStampFile (paramItems[0], paramItems[1], paramItems[2], paramItems[3][0], skipTime);
          }
        }
      }
      if (null == result) {
        if (log.IsDebugEnabled) {
          log.Debug ($"GetStringValueFromStampFile: no value for param {param}");
        }
        throw new Exception ("No value for param" + param);
      }
      return result;
    }

    /// <summary>
    /// get value from a stamp file.
    /// </summary>
    /// <param name="pathName"></param>
    /// <param name="mainStampFile"></param>
    /// <param name="stampFilePrefix"></param>
    /// <param name="separator"></param>
    /// <param name="skipTime"></param>
    /// <returns></returns>
    [Obsolete("Remove the implementation since there was some weakness in the implementation", true)]
    string GetStringValueFromStampFile (String pathName, string mainStampFile, string stampFilePrefix, char separator, int skipTime)
    {
      // There are some implementation weakness. For example, the cache value was badly managed
      // Re-implement it only if needed in the future
      throw new NotImplementedException ("Remove the implementation since there was some weakness in the implementation");
    }

    /// <summary>
    /// Get the list of files in a folder
    /// </summary>
    /// <param name="distantDir"></param>
    /// <returns></returns>
    IEnumerable<string> GetFileList (string distantDir)
    {
      if (log.IsDebugEnabled) {
        log.Debug ($"GetFileList distantDir={distantDir}");
      }

      var pAttributeSelection = new HeidenhainDNCLib.JHFileAttributes ();
      var pAttributeState = new HeidenhainDNCLib.JHFileAttributes ();

      var directoryList = m_interfaceManager.InterfaceFileSystem.ReadDirectory (distantDir,
        pAttributeSelection, pAttributeState);

      int directoryCount = directoryList.Count;
      for (int index = 0; index < directoryCount; index++) {
        yield return directoryList[index].name;
      }
    }

    /// <summary>
    /// Get the list of files in a folder with datetime
    /// </summary>
    /// <param name="distantDir"></param>
    /// <returns></returns>
    IEnumerable<Tuple<string, DateTime>> GetFileListWithDatetime (string distantDir)
    {
      if (log.IsDebugEnabled) {
        log.Debug ($"GetFileListWithDatetime distantDir={distantDir}");
      }

      var pAttributeSelection = new HeidenhainDNCLib.JHFileAttributes ();
      var pAttributeState = new HeidenhainDNCLib.JHFileAttributes ();

      var directoryList = m_interfaceManager.InterfaceFileSystem
        .ReadDirectory (distantDir, pAttributeSelection, pAttributeState);

      int directoryCount = directoryList.Count;
      for (int index = 0; index < directoryCount; index++) {
        var fileName = directoryList[index].name;
        var fileDateTime = directoryList[index].dateTime;
        if (log.IsDebugEnabled) {
          log.Debug ($"GetFileListWithDatetime: filename={fileName}, DateTime={fileDateTime}");
        }
        if (!fileName.Equals ("..") && !fileName.Equals ("PLC:")) {
          yield return new Tuple<string, DateTime> (fileName, fileDateTime);
        }
      }
    }

    /// <summary>
    /// Delete files not containing a given pattern in a directory
    /// </summary>
    /// <param name="directoryPath">directory path</param>
    /// <param name="fileList"></param>
    /// <param name="filePatternToKeep"></param>
    /// <returns></returns>
    void DeleteRemoteFiles (string directoryPath, IEnumerable<Tuple<string, DateTime>> fileList, string filePatternToKeep)
    {
      log.Debug ("DeleteRemoteFiles");
      foreach (var fileItem in fileList) {
        if (!(fileItem.Item1).Contains (filePatternToKeep)) {
          string fullFileName = Path.Combine (directoryPath, fileItem.Item1);
          if (log.IsDebugEnabled) {
            log.Debug ($"DeleteRemoteFiles: file={fullFileName}");
          }
          m_interfaceManager.InterfaceFileSystem.DeleteFile (fullFileName);
        }
      }
    }

    /// <summary>
    /// return current directory
    /// </summary>
    public string GetCurrentDirectory ()
    {
      var result = m_interfaceManager.InterfaceFileSystem.GetCurrentDirectory ();
      log.Debug ($"GetCurrentDirectory: dir={result}");
      return result;
    }

    /// <summary>
    /// change current directory
    /// </summary>
    /// <param name="directoryName"></param>
    public void ChangeDirectory (string directoryName)
    {
      log.DebugFormat ("ChangeDirectory: dir={0}", directoryName);
      m_interfaceManager.InterfaceFileSystem.ChangeDirectory (directoryName);
    }

    /// <summary>
    /// Get a value from a stamp file name
    /// 
    /// Extract value from the latest file in the specified folder
    /// 
    /// Files format: no specific format. Filename=value
    /// 
    /// Folder cleaning: after program change, files in folder with different cncfile are deleted if second paramter is true
    /// 
    /// Query to remote folder is done only at read interval (in s)
    /// 
    /// If suffix is specified it will be removed from filename
    /// </summary>
    /// <param name="param">list with separator: ;directory path;delete remote file;file read time interval in s;suffix</param>
    /// <returns></returns>
    public double GetValueFromFileName (string param)
    {
      var s = GetStringValueFromFileName (param);
      try {
        return double.Parse (s, System.Globalization.CultureInfo.InvariantCulture);
      }
      catch (Exception ex) {
        log.Error ($"GetValueFromFileName: {s} is not a double", ex);
        throw;
      }
    }

    /// <summary>
    /// Get a value from a stamp file name
    /// 
    /// Extract value from the latest file in the specified folder
    /// 
    /// Files format: no specific format. Filename=value
    /// 
    /// Folder cleaning: after program change, files in folder with different cncfile are deleted if second paramter is true
    /// or if a maximum number of files is reached in the folder
    /// 
    /// Query to remote folder is done only at read interval (in s)
    /// 
    /// If suffix is specified it will be removed from filename
    /// </summary>
    /// <param name="param">list with separator: ;directory path;max file number or delete remote files;file read time interval in s;suffix</param>
    /// <returns></returns>
    public string GetStringValueFromFileName (string param)
    {
      if (log.IsDebugEnabled) {
        log.Debug ($"GetStringValueFromFileName: param={param}");
      }

      if (string.IsNullOrEmpty (param)) {
        log.ErrorFormat ("GetStringValueFromFileName: empty parameter");
        throw new ArgumentNullException ("param", "Empty parameter");
      }
      else {
        var paramItems = Lemoine.Collections.EnumerableString.ParseListString (param);
        if (paramItems.Length < 3 || paramItems.Length > 4) {
          log.Error ("GetStringValueFromFileName: bad number of parameters");
          throw new ArgumentException ("Bad number of parameters", "param");
        }
        else {
          if (!int.TryParse (paramItems[2], out var skipTimeSeconds)) {
            log.ErrorFormat ("GetStringValueFromFileName: invalid time parameter");
            throw new ArgumentException ("Invalid time parameter", "param");
          }
          else {
            string suffix = null;
            if (paramItems.Length == 4) {
              suffix = paramItems[3];
            }
            if (!int.TryParse (paramItems[1], out var maxFileNumber)) {
              if (bool.TryParse (paramItems[1], out var deleteRemoteFiles)) {
                maxFileNumber = deleteRemoteFiles ? 1 : int.MaxValue;
              }
              else {
                maxFileNumber = int.MaxValue;
              }
            }
            var skipTime = TimeSpan.FromSeconds (skipTimeSeconds);
            return GetStringValueFromFileName (paramItems[0], maxFileNumber, skipTime, suffix);
          }
        }
      }
    }

    /// <summary>
    /// get value from a stamp file name
    /// </summary>
    /// <param name="directoryPath">remote folder path</param>
    /// <param name="maxFileNumber">delete remote files when the max file number is reached</param>
    /// <param name="skipTime">delay between 2 DNC interface queries</param>
    /// <param name="suffix">string to be removed from value</param>
    /// <returns></returns>
    string GetStringValueFromFileName (string directoryPath, int maxFileNumber, TimeSpan skipTime, string suffix)
    {
      if (log.IsDebugEnabled) {
        log.Debug ($"GetStringValueFromFileName: directoryPath={directoryPath}");
      }

      // Check if already in list
      DateTime lastGetDataTime;
      if (m_cacheDateTime.ContainsKey (directoryPath)) {
        lastGetDataTime = m_cacheDateTime[directoryPath];
        if (DateTime.Now.Subtract (lastGetDataTime) < skipTime) {
          if (log.IsDebugEnabled) {
            log.Debug ("GetStringValueFromFileName: poll interval too short");
            if (m_cacheData.TryGetValue (directoryPath, out var lastResult)) {
              return lastResult;
            }
            else {
              log.Error ($"GetStringValueFromFileName: no last result although there is a last date/time");
            }
          }
        }
      }

      // Get latest file
      var remoteFileList = GetFileListWithDatetime (directoryPath).ToList ();
      var fileName = remoteFileList
        .OrderBy (t => t.Item2)
        .LastOrDefault ()?.Item1;
      if (!string.IsNullOrEmpty (fileName)) {
        if (log.IsDebugEnabled) {
          log.Debug ($"GetStringValueFromFileName: latest file name={fileName}");
        }
        // remove suffix if it matches configuration
        string result;
        var fileSuffix = Path.GetExtension (fileName);
        if (fileSuffix.Equals (suffix, StringComparison.InvariantCultureIgnoreCase)) {
          result = Path.GetFileNameWithoutExtension (fileName);
        }
        else {
          result = fileName;
        }

        m_cacheDateTime[directoryPath] = DateTime.Now;
        m_cacheData[directoryPath] = result;

        try {
          if (maxFileNumber < remoteFileList.Count) {
            if (log.IsDebugEnabled) {
              log.Debug ($"GetStringValueFromFileName: remove remote files since max file number in parameter {maxFileNumber} is reached");
            }
            DeleteRemoteFiles (directoryPath, remoteFileList, fileName);
          }
          else {
            var maxFileNumberConfig = Lemoine.Info.ConfigSet
              .LoadAndGet (REMOVE_REMOTE_FILES_MAX_FILE_NUMBER_KEY, REMOVE_REMOTE_FILES_MAX_FILE_NUMBER_DEFAULT);
            if (maxFileNumberConfig < remoteFileList.Count) {
              if (log.IsDebugEnabled) {
                log.Debug ($"GetStringValueFromFileName: remove remote files since max file number in config {maxFileNumberConfig} is reached");
              }
              DeleteRemoteFiles (directoryPath, remoteFileList, fileName);
            }
          }
        }
        catch (Exception ex) {
          log.Error ($"GetStringValueFromFileName: delete remote files failed", ex);
        }

        DeleteRemoteFiles (directoryPath, remoteFileList, fileName);
        return result;
      }
      else {
        if (log.IsDebugEnabled) {
          log.Debug ("GetStringValueFromFileName: no file in folder");
        }
        throw new Exception ("No file in folder => no value");
      }
    }

    #endregion // Get methods
  }
}
