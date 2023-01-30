// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: GPL-2.0-or-later

API(int, LSV2Open, (void**, const char*, unsigned long*, int));
API(int, LSV2Close, (void*));
API(int, LSV2Login, (void*, LPCSTR, LPCSTR));
API(int, LSV2Logout, (void*, LPCSTR));
API(int, LSV2SetBlockHook, (void*, void*));
API(int, LSV2GetErrStringEx, (void*, DWORD, LPSTR, DWORD*, DWORD));
API(int, LSV2ReceivePara, (void*, void*));
API(int, LSV2ReceiveMem, (void*, DWORD, DWORD, LPBYTE));
API(int, LSV2GetErrString, (void*, DWORD, DWORD));
API(int, LSV2ReceiveRunInfo, (void*, LSV2_RUNINFO_TYPE, LSV2RUNINFO*));
API(int, LSV2ReceiveVersions, (HANDLE hPort, LPSTR lpModel, LPSTR lpNCVersion, LPSTR lpPLCVersion, LPSTR lpOptVersion));
API(int, LSV2GetTCPErrorDetails, (LPSTR, DWORD*));
API(int, LSV2ReceiveTableLine, (HANDLE hPort, LPCSTR fileName, LPCSTR sqlQuery, LPSTR lineBuf, DWORD maxLineBuf, DWORD startLine));
API(int, LSV2ReceiveTableLineEx, (HANDLE hPort, LPCSTR fileName, LPCSTR sqlQuery, LPSTR lineBuf, DWORD maxLineBuf, DWORD* foundLine, DWORD startLine));
API(int, LSV2ReceiveFile, (HANDLE hPort, LPCSTR srcFile, LPCSTR destFile, BOOL canOverwrite, DWORD mode));
API(int, LSV2ReceiveFileInfo, (HANDLE hPort, LPCSTR FileName, struct _finddata_t* lpEntry));
API(int, LSV2ReceiveDataProperty, (HANDLE hPort, LPCSTR entryName, LSV2PROPKIND proeprtyType, LSV2DATA* pData));
API(int, LSV2ReceiveMachineConstant, (HANDLE hPort, LPCSTR entryName, LPSTR buffer, DWORD bufferLength));
API(int, LSV2ChangeDir, (HANDLE hPort, LPCSTR PathName));
API(int, LSV2ReceiveDir, (HANDLE hPort, DWORD* DirSize, DWORD* DirCount));
API(int, LSV2ReceiveDirInfo, (HANDLE hPort, DIRDATA* lpDirData));
API(struct _finddata_t*, LSV2GetDirEntry, (HANDLE hPort, enum LSV2_ACCESS_TYPE Access));
API(int, LSV2DeleteFile, (HANDLE hPort, LPCSTR FileName));
API(int, LSV2RenameFile, (HANDLE hPort, LPCSTR FileName, LPCSTR NewFileName));
