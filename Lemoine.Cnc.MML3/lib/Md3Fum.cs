using System;
using System.Runtime.InteropServices;

namespace Lemoine.Cnc
{
  /// <summary>
  /// Interface to the library Md3Fum.dll
  /// </summary>
  static public class Md3Fum
  {
    /// <summary>
    /// Get current registory information.
    /// - When each data is not necessary, set NULL in each argument.
    /// </summary>
    /// <param name="section">LPCSTR	section	: name of subkey to open.</param>
    /// <param name="subKeyTotal">LPDWORD	subKeyTotal : address of buffer for number of subkeys.</param>
    /// <param name="subKeyMaxLen">LPDWORD	subKeyMaxLen: address of buffer for longest subkey name length.</param>
    /// <param name="valueTotal">ref UInt32 valueTotal	: address of buffer for number of value entries.</param>
    /// <param name="valueNameLen">LPDWORD	valueNameLen: address of buffer for longest value name length.</param>
    /// <param name="valueLen">LPDWORD	valueLen	: address of buffer for longest value data length.</param>
    /// <returns>TRUE / FALSE(Session does not exist):</returns>
    [DllImport ("Md3Fum.dll", EntryPoint = "md3fum_reg_information",
               SetLastError = true, CharSet = CharSet.Ansi)]
    public static extern bool RegInformation (string section, out UInt32 subKeyTotal,
                                             out UInt32 subKeyMaxLen, out UInt32 valueTotal,
                                             out UInt32 valueNameLen, out UInt32 valueLen);

    //    [DllImport("Md3Fum.dll", EntryPoint="md3fum_reg_get_value_name",
    //               SetLastError = true, CharSet = CharSet.Ansi)]
    //    public static extern bool reg_get_value_name(string section, UInt32 dwIndex,
    //                                                 string valName, short valLen,
    //                                                 string valData, short dataLen);
    //	[Function]
    //		Get value name from enumarate index.
    //	[Argument]
    //		LPCSTR	section	: address of name of subkey to open.
    //		DWORD	dwIndex		: index of value to query.
    //		LPTSTR	valName		: address of buffer for value string.
    //		short	valLen		: value length.
    //		LPTSTR	valData		: address of buffer for value data.
    //		short	dataLen		: data length.
    //	[Result]
    //		TRUE	:value is existed.
    //		FALSE	:cannot find.

    //    [DllImport("Md3Fum.dll", EntryPoint="md3fum_reg_get_value_index",
    //               SetLastError = true, CharSet = CharSet.Ansi)]
    //    public static extern bool reg_get_value_index(string section, string compValueName,
    //                                                  string valData, short valLen, out UInt32 index);
    //	[Function]
    //		Get index of value to enumarate from session name.
    //	[Argument]
    //		LPCSTR	section		: address of name of subkey to open.
    //		LPCSTR	compValueName	: compared value name
    //		LPTSTR	valData			: address of buffer for value data.
    //		short	valLen			: value length.
    //		ref UInt32 index			: index of value to enumarate.
    //	[Result]
    //		TRUE / FALSE

    //    [DllImport("Md3Fum.dll", EntryPoint="md3fum_reg_get_key_index",
    //               SetLastError = true, CharSet = CharSet.Ansi)]
    //    public static extern bool reg_get_key_index(string section, string compKeyName, out UInt32 index);
    //	[Function]
    //		Get index of subkey to enumarate from session name.
    //	[Argument]
    //		LPCSTR	section	: address of name of subkey to open.
    //		LPCSTR	compKeyName	: compared subkey name
    //		ref UInt32 index		: index of subkey to enumarate.
    //	[Result]
    //		TRUE / FALSE

    //    [DllImport("Md3Fum.dll", EntryPoint="md3fum_reg_get_key_name",
    //               SetLastError = true, CharSet = CharSet.Ansi)]
    //    public static extern bool reg_get_key_name(string section, UInt32 dwIndex, string keyName, UInt32 keyLen);
    //	[Function]
    //		Get subkey name from enumarate index.
    //	[Argument]
    //		LPCSTR	section	: address of name of subkey to open.
    //		DWORD	dwIndex		: index of value to query.
    //		LPSTR	keyName		: address of buffer of target key name.
    //		short	keyLen		: keyName length
    //	[Result]
    //		FALSE	:section does not exist.
    //		TRUE	:

    //    [DllImport("Md3Fum.dll", EntryPoint="md3fum_set_emulate_data",
    //               SetLastError = true, CharSet = CharSet.Ansi)]
    //    public static extern bool set_emulate_data(string section, string keyName, string data);
    //	[Function]
    //		Write data to INI file for Emulator.
    //	[Arguments]
    //		section     : Application name or common key name.
    //		keyName     : Key name string.
    //		data        : Data value string.
    //	[Return]
    //		TRUE / FALSE
    //	[Remarks]
    //		FALSE return when data name or section not found, INI file write error.

    //    [DllImport("Md3Fum.dll", EntryPoint="md3fum_get_emulate_data",
    //               SetLastError = true, CharSet = CharSet.Ansi)]
    //    public static extern bool get_emulate_data(string section, string keyName, string data, UInt32 bufLen);
    //	[Function]
    //		Read INI file data for Emulator.
    //	[Arguments]
    //		section     : Application name or common key name.
    //		keyName     : Data name string.
    //		data        : Data value string.
    //		bufSize     : Buffer size.
    //	[Return]
    //		TRUE / FALSE
    //	[Remarks]
    //		FALSE return back when data name or section not found, INI file read error.

    //    [DllImport("Md3Fum.dll", EntryPoint="md3fum_delete_emulate_section",
    //               SetLastError = true, CharSet = CharSet.Ansi)]
    //    public static extern bool delete_emulate_section(string sectionPath, string section);
    //	[Function]
    //		Delete specified section of INI file.
    //	[Arguments]
    //		sectionPath : Application name or common key name.
    //		section     : delete section.
    //	[Return]
    //		TRUE / FALSE

    //    [DllImport("Md3Fum.dll", EntryPoint="md3fum_delete_emulate_key",
    //               SetLastError = true, CharSet = CharSet.Ansi)]
    //    public static extern bool delete_emulate_key(string section, string keyName);
    //	[Function]
    //		Delete specified key name of INI file (registory).
    //	[Arguments]
    //		section     : Application name or common key name.
    //		keyName     : key name string.
    //	[Return]
    //		TRUE / FALSE

    //    [DllImport("Md3Fum.dll", EntryPoint="md3fum_get_emulate_all_section",
    //               SetLastError = true, CharSet = CharSet.Ansi)]
    //    public static extern bool get_emulate_all_section(string section, out UInt32 sectionTotal, out UInt32 sectionMaxLen);
    //	[Function]
    //		Get total of specified INI file section.(Only section have multi key names.)
    //	[Arguments]
    //		section       : Application name or common key name.
    //		sectionTotal  : Total of specified INI file section.
    //		sectionMaxLen : Longest section name length of specified INI file section.
    //	[Return]
    //		TRUE / FALSE
    //	[Remarks]
    //		- When each data is not necessary, set NULL in each argument.

    //    [DllImport("Md3Fum.dll", EntryPoint="md3fum_get_emulate_word_from_section",
    //               SetLastError = true, CharSet = CharSet.Ansi)]
    //    public static extern bool get_emulate_word_from_section(string section, UInt32 dwIndex, string valueName, UInt32 valueLen);
    //	[Function]
    //		Get word after parameter section from specified INI file section.
    //	[Argument]
    //		LPCSTR	section    : Application name or common key name.
    //		DWORD	dwIndex    : Index of value.
    //		LPSTR	valueName  : The word from specified INI file section.
    //		short	valueLen   : Word length.
    //	[Return]
    //		TRUE / FALSE

    //    [DllImport("Md3Fum.dll", EntryPoint="md3fum_get_emulate_value_from_section",
    //               SetLastError = true, CharSet = CharSet.Ansi)]
    //    public static extern bool get_emulate_value_from_section(string section, UInt32 dwIndex, string keyName,
    //                                                             short keyNameLen, string valueData, short valueLen);
    //	[Function]
    //		Get key name and key value from specified INI file (registory) section.
    //	[Argument]
    //		LPCSTR	section      : Application name or common key name.
    //		DWORD	dwIndex      : Index of key name.
    //		LPTSTR	keyName      : Key name in specified INI file section.
    //		short	keyNameLen   : Key name length.
    //		LPTSTR	keyValue     : Key value.
    //		short	keyValueLen  : Key value length.
    //	[Return]
    //		TRUE / FALSE

    //    [DllImport("Md3Fum.dll", EntryPoint="md3fum_split_path",
    //               SetLastError = true, CharSet = CharSet.Ansi)]
    //    public static extern bool split_path(string fullName,
    //                                         string devName, string dirName, string fName, string extName);
    //	[Function]
    //		It is function that Full path file name string is decomposed into device, directory,
    //		file name, and file type (extension).
    //	[Arguments]
    //		string fullName : Full-path file name string
    //		string  devName  : Device name string
    //		string  dirName  : Directory name string
    //		string  fName    : File name string
    //		string  extName  : File extention string.
    //	[Return]
    //		TRUE / FALSE
    //	[Remarks]
    //		- NULL is set in the argument not acquired.
    //		- FALSE returns when each string length of the device, the directory, the file name,
    //			and file type (extension) is more than predetermined value.
    //		- The character string of 0 in length returns to the device name
    //			for the file name of the ...\...\ format.

    //    [DllImport("Md3Fum.dll", EntryPoint="md3fum_log_file",
    //               SetLastError = true, CharSet = CharSet.Ansi)]
    //    public static extern bool log_file(string logFileName, string errMsg);
    //	[Function]
    //		It is function for messagre write down to log file.
    //		If any applications access to same log file, that jobs are serialized by semaphore.
    //	[Arguments]
    //		logFileName : log file name.
    //		errMsg      : Write down data string.
    //	[Return]
    //		TRUE / FALSE
    //		TRUE / FALSE
    //	[Remarks]
    //		FALSE return back when file R/W error occured.

    //    public static extern void errlog(Int32 nodeNo, short Type, string Module, Int32 (*FuncPtr)(), Int32 Err, Int32 SubErr, string msg,...);
    //	[Function]
    //		Write error information to log file.
    //	[Arguments]
    //		long	nodeNo	: Node number of HSSB
    //		short 	Type	:	<Type>		<LogWrite>		<Process>
    //							WARN_SYS		Yes			Continue
    //							FATL_SYS		Yes			Exit
    //							INFO_SYS		Yes			Continue
    //		string 	Module  : Module name (source file name).
    //		LPVOID	FuncPtr	: Function address of setLastError routine.
    //		long	Err     : Error number in this module.
    //		long	SubErr	: Sub error number
    //		LPCSTR	msg,...	: Free area for module creater to use (Comment, sub error no. etc..)
    //	[Return]

    //    [DllImport("Md3Fum.dll", EntryPoint="md3fum_chk_remote_file",
    //               SetLastError = true, CharSet = CharSet.Ansi)]
    //    public static extern bool chk_remote_file(string name);
    //	[Function]
    //		Check whether specified file on remote disk drive or not.
    //	[Arguments]
    //		string name : Checked file name.
    //	[Return]
    //		TRUE(File in remote device) / FALSE
    //	[Remark]
    //		Return TRUE in the case specified file on removable disk.

    //    [DllImport("Md3Fum.dll", EntryPoint="md3fum_dir_string",
    //               SetLastError = true, CharSet = CharSet.Ansi)]
    //    public static extern bool dir_string(string buf);
    //	[Function]
    //		It is Function which \ mark is added at the end of string.
    //	[Arguments]
    //		string buf : Character string to which \ mark is added
    //	[Return]
    //		TRUE/FALSE
    //	[Remarks]
    //		- FALSE return back when length of string in "buf" is over length.
    //		- When the end of the character string is \, \ mark is not added.

    //    public static extern HANDLE create_mutex(string mutexName);

    //    public static extern bool release_mutex(HANDLE mutexHandle);

    //    [DllImport("Md3Fum.dll", EntryPoint="md3fum_get_version_item",
    //               SetLastError = true, CharSet = CharSet.Ansi)]
    //    public static extern bool get_version_item(string fname, string keyword, string GetStr);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="fname"></param>
    /// <param name="company"></param>
    /// <param name="description"></param>
    /// <param name="fileVer"></param>
    /// <param name="internl"></param>
    /// <param name="copyright"></param>
    /// <param name="original"></param>
    /// <param name="productName"></param>
    /// <param name="productVer"></param>
    /// <returns></returns>
    [DllImport ("Md3Fum.dll", EntryPoint = "md3fum_get_version",
               SetLastError = true, CharSet = CharSet.Ansi)]
    public static extern bool GetVersion (string fname, string company, string description, string fileVer,
                                         string internl, string copyright, string original, string productName, string productVer);

    //    public static extern UInt32 set_allocMem(HGLOBAL *hMem, UInt32 allocBytes, char *data);
    //	[Function]
    //		This routine set the specified data to the heap.
    //	[Argument]
    //		HGLOBAL	*hMem
    //			The handle of the newly allocated memory object.
    //		DWORD	allocBytes
    //			Specifies the number of bytes to allocate. Same as data size.
    //		char	*data
    //			pointer of data.
    //			the newly allocated memory area is setted from data.
    //	[Return]
    //		If the function succeeds, the return value is 0.
    //		If the function fails, the return value is GetLastError() for
    //		GlobalAlloc() or GlobalLock().
    //	[Remark]
    //		set_allocMem() and get_allocMem() make a pair.
    //		Thread arguments, PostMessage arguments,...

    //    public static extern UInt32 get_allocMem(HGLOBAL hMem, UInt32 allocBytes, char *data);
    //	[Function]
    //		This routine gets the specified data from the heap.
    //	[Argument]
    //		HGLOBAL	hMem
    //			The handle of the allocated memory object.
    //		DWORD	allocBytes
    //			Specifies the number of bytes allocated memory.
    //		char	*data
    //			pointer of data.
    //			Data is setted form allocated memory area data.
    //	[Return]
    //		If the function succeeds, the return value is 0.
    //		If the function fails, the return value is GetLastError() for
    //		GlobalLock().
    //	[Remak]
    //		set_allocMem() and get_allocMem() make a pair.
    //		Thread arguments, PostMessage arguments,...

    //    public static extern UInt32 thread_stop_request(char *semName, HANDLE hThread, UInt32 timeout);
    //	[Function]
    //		This routine request to stop thread.
    //	[Argument]
    //		char	*semName
    //			The name of semaphore.
    //		HANDLE	hThread
    //			It was made to stop thread handle.
    //	[Return]
    //		If the function succeeds, the return value is 0.
    //		If the function fails, the return value is GetLastError() for
    //		OpenSemaphore() or ReleaseSemaphore().
    //	[Remak]
    //		CreateSemaphore() and WaitSingleObject() make a pair
    //		in stopped thread.

    //    public static extern UInt32 check_thread(HANDLE hThread, bool *stat);
    //	[Function]
    //		This routine check thread executing.
    //	[Argument]
    //		HANDLE	hThread
    //			checked thread handle
    //		BOOL	*stat
    //			TRUE:executing, FALSE:stop or not exist or error
    //	[Return]
    //		If the function succeeds, the return value is 0.
    //		If the function fails, the return value is GetLastError() for
    //		GetExitCodeThread().

    //    public static extern bool unittostr(string unitStr, const unit_t *unit);
    //	[Function]
    //		convert the UNIT type to string
    //	[Argument]
    //		LPSTR	unitStr
    //					The converted string
    //					need UNIT_STR_S characters (include NULL)
    //		unit_t	*unit
    //					UNIT to convert
    //	[Return]
    //		If the function succeeds, the return value is TRUE.
    //		If the function fails, the return value is FALSE.

    //    public static extern bool strtounit(string unitStr, unit_t *unit);
    //	[Function]
    //		convert the string to UNIT type
    //	[Argument]
    //		LPCSTR	unitStr
    //					string to convert
    //					need UNIT_STR_S characters (include NULL)
    //		unit_t	*unit
    //					The converted UNIT type
    //	[Return]
    //		If the function succeeds, the return value is TRUE.
    //		If the function fails, the return value is FALSE.

    //    public static extern UInt16 strArray2variant(VARIANT *var, char **fromStr, UInt16 numStr);

    //    [DllImport("Md3Fum.dll", EntryPoint="md3fum_chk_bit_range",
    //               SetLastError = true, CharSet = CharSet.Ansi)]
    //    public static extern bool chk_bit_range(UInt16 bitNo);

    //    [DllImport("Md3Fum.dll", EntryPoint="md3fum_get_platform",
    //               SetLastError = true, CharSet = CharSet.Ansi)]
    //    public static extern bool get_platform(UInt32 platform);
    //	[Function]
    //		get computer platform information
    //	[Argument]
    //		platform	checking platform type (winbase.h)
    //					VER_PLATFORM_WIN32s             0
    //					VER_PLATFORM_WIN32_WINDOWS      1
    //					VER_PLATFORM_WIN32_NT           2
    //	[Return]
    //		If platform is checking platform type, the return value is TRUE.

    //    [DllImport("Md3Fum.dll", EntryPoint="md3fum_shutdown",
    //               SetLastError = true, CharSet = CharSet.Ansi)]
    //    public static extern UInt32 shutdown(UInt32 type, string msg, UInt32 timeout);
    //	[Function]
    //		exit windows function
    //	[Argument]
    //		type	shutdown type (winuser.h)
    //				EWX_LOGOFF   0
    //				EWX_SHUTDOWN 1
    //				EWX_REBOOT   2
    //				EWX_FORCE    4
    //				EWX_POWEROFF 8
    //		msg		Reserve
    //		timeout	Reserve
    //	[Return]
    //		If the function succeeds, the return value is zero.
    //		If the function fails, the return value is GetLastError().

    //    public static extern HANDLE wait_mutex(string mutexName);

    //    [DllImport("Md3Fum.dll", EntryPoint="md3fum_ini_get",
    //               SetLastError = true, CharSet = CharSet.Ansi)]
    //    public static extern string ini_get(string sectionName,
    //                                        string keyName,
    //                                        string defString,
    //                                        string returnedValue,
    //                                        Int32 returnedValueSize,
    //                                        string iniFileName);

    //    [DllImport("Md3Fum.dll", EntryPoint="md3fum_ini_set",
    //               SetLastError = true, CharSet = CharSet.Ansi)]
    //    public static extern void ini_set(string sectionName,
    //                                      string keyName,
    //                                      string settingValue,
    //                                      string iniFileName);

    //    [DllImport("Md3Fum.dll", EntryPoint="md3fum_makino_data_dir",
    //               SetLastError = true, CharSet = CharSet.Ansi)]
    //    public static extern bool makino_data_dir(string bufStr, UInt16 bufLen);

    //    public static extern bool get_comment_t_code(
    //      string lineStr, char *ptn, char *ftn, char *count, char *used);

    //    public static extern bool get_comment_t_code_ftn_name(
    //      string lineStr, char *ptn, char *ftnName, char *count, char *used);

    //    public static extern UInt32 remote_shutdown(string rpc_name,...);
    //	[Function]
    //		exit remote windows function
    //	[Argument]
    //		rpc_name	Shutdown remote PC name (or IP address)
    //					ex.) "mosatcpc001", "192.168.1.1"
    //		Optional1	msg.....Shutdown message = ""
    //		Optional2	delay...Shutdown delay time = 0 [Sec.]
    //		Optional3	force...Force shutdown = True
    //		Optional4	reboot..Reboot after shutdown = False
    //	[Return]
    //		If the function succeeds, the return value is zero.
    //		If the function fails, the return value is GetLastError().

    //    [DllImport("Md3Fum.dll", EntryPoint="md3fum_commlog",
    //               SetLastError = true, CharSet = CharSet.Ansi)]
    //    public static extern void commlog(Int32 nodeNo, string category, string msg);
    //	[Function]
    //		Write communication data information to communication log file.
    //	[Arguments]
    //		nodeNo		: node number.
    //		category	: communication data category.
    //		msg			: Write down data string.
    //	[Return]

    //    public static extern void split_node_info(string nodeInfo,
    //                                              Int32 *node, string ipAddress, out UInt16 port, bool *emulate);
    //	[Function]
    //		Get HANDLE corresponding to node_info.
    //	[Arguments]
    //		nodeInfo	: "nodeNo/IP address/tcpPort/e" (e:1-EmulateMode,0-ReleaseMode)
    //		node		: nodeNo
    //		ipAddress	: IP address
    //		port		: tcp Port number
    //		emulate		: TRUE(Emulate Mode)/FALSE(Release Mode)

    //    public static extern bool LPSTR2BSTR(string srcStr, BSTR *destStr);

    //    [DllImport("Md3Fum.dll", EntryPoint="md3fum_chk_HssbConnect",
    //               SetLastError = true, CharSet = CharSet.Ansi)]
    //    public static extern bool chk_HssbConnect(Int32 node);

    //    [DllImport("Md3Fum.dll", EntryPoint="md3fum_chk_EthernetConnect",
    //               SetLastError = true, CharSet = CharSet.Ansi)]
    //    public static extern bool chk_EthernetConnect(Int32 node);
  }
}
