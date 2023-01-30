// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: GPL-2.0-or-later

#pragma once

#include <Windows.h>

#include "lsv2/lsv2_dnc_def.h"
#include "lsv2/lsv2_data_def.h"
#include "lsv2/lsv2_data.h"

using namespace System;
using namespace System::Collections::Generic;
using namespace Lemoine::Core::Log;

namespace Lemoine
{
	namespace Cnc
	{
		public ref class ToolData
		{
		public:
			ToolData();

			void SetNumber(int number);
			int GetNumber();

			void SetName(String^ name);
			String^ GetName();

			void SetCurrent(double current);
			double GetCurrent();

			Nullable<double> m_warning;
			Nullable<double> m_limit;
			Nullable<double> m_compensationD;
			Nullable<double> m_compensationH;

			bool IsValid();
			List<String^>^ MissingVariables();

		private:
			bool m_otherReasonNotToBeOk;

			int m_number;
			bool m_numberProvided;

			String^ m_name;
			bool m_nameProvided;

			double m_current;
			bool m_currentProvided;
		};
	}
}