// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: GPL-2.0-or-later

#include "ToolData.h"

namespace Lemoine
{
	namespace Cnc
	{
		ToolData::ToolData() :
			m_warning(Nullable<double>()),
			m_limit(Nullable<double>()),
			m_compensationD(Nullable<double>()),
			m_compensationH(Nullable<double>()),
			m_otherReasonNotToBeOk(false),
			m_number(0),
			m_numberProvided(false),
			m_name(""),
			m_nameProvided(false),
			m_current(0.0),
			m_currentProvided(false)
		{}

		void ToolData::SetNumber(int number)
		{
			m_number = number;
			m_numberProvided = true;
			m_otherReasonNotToBeOk |= (number == 0);
		}

		int ToolData::GetNumber()
		{
			return m_number;
		}

		void ToolData::SetName(String^ name)
		{
			m_name = name;
			m_nameProvided = true;
			m_otherReasonNotToBeOk |= (name == "NULLWERKZEUG");
		}

		String^ ToolData::GetName()
		{
			return m_name;
		}

		void ToolData::SetCurrent(double current)
		{
			m_current = current;
			m_currentProvided = true;
		}

		double ToolData::GetCurrent()
		{
			return m_current;
		}

		bool ToolData::IsValid()
		{
			return m_numberProvided && m_nameProvided && m_currentProvided && !m_otherReasonNotToBeOk;
		}

		List<String^>^ ToolData::MissingVariables()
		{
			List<String^>^ list = gcnew List<String^>();

			if (!m_numberProvided)
				list->Add("T");
			if (!m_nameProvided)
				list->Add("NAME");
			if (!m_currentProvided)
				list->Add("CUR_TIME or CUR.TIME");

			return list;
		}
	}
}