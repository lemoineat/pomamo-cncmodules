// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: GPL-2.0-or-later

#ifndef _SELGECSTRUCT_H_
#define _SELGECSTRUCT_H_

#include "SeLGeC/TripData.h"

typedef void	(FAR *SLPROC_PROCESSTCPMSG)(LPARAM lParam, int code);

#define	WM_SL_PROCESSTCPMSG	WM_USER + 1

const	WORD	SLC_MSG			= 1;
const	WORD	SLC_DISCONNECT	= 2;

typedef	struct {
	short		sloid;	// SLO ID or ID of destination PP
	short		dest;	// (DEV_*) destination code
	Dato_t		dd;		// datum
}SLMsg_t; 

struct SLODato_t{
	long				d_dword;
	double				d_double;
	char				d_string[MAXDATASLEN];
	EuStreamPtrInfo_t	*d_p;
};

#endif // _SELGECSTRUCT_H_
