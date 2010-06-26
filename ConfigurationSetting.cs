﻿/****************************************************************************
 *   This file is part of Helpmebot.                                        *
 *                                                                          *
 *   Helpmebot is free software: you can redistribute it and/or modify      *
 *   it under the terms of the GNU General Public License as published by   *
 *   the Free Software Foundation, either version 3 of the License, or      *
 *   (at your option) any later version.                                    *
 *                                                                          *
 *   Helpmebot is distributed in the hope that it will be useful,           *
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of         *
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the          *
 *   GNU General Public License for more details.                           *
 *                                                                          *
 *   You should have received a copy of the GNU General Public License      *
 *   along with Helpmebot.  If not, see <http://www.gnu.org/licenses/>.     *
 ****************************************************************************/

#region Usings

using System;

#endregion

namespace helpmebot6
{
    public class ConfigurationSetting
    {
        private const double CACHE_TIMEOUT = 5;

        private string _settingValue;
        private readonly string _settingName;
        private DateTime _lastRetrieval;

        public ConfigurationSetting(string name, string value)
        {
            _settingName = name;
            _settingValue = value;
            _lastRetrieval = DateTime.Now;
        }

        public bool isValid()
        {
            try
            {
                TimeSpan difference = DateTime.Now - _lastRetrieval;
                return difference.TotalMinutes <= CACHE_TIMEOUT;
            }
            catch (Exception ex)
            {
                GlobalFunctions.errorLog(ex);
            }
            return false;
        }

        public string value
        {
            get { return _settingValue; }
            set
            {
                _settingValue = value;
                _lastRetrieval = DateTime.Now;
            }
        }

        public string name
        {
            get { return _settingName; }
        }

        public override string ToString()
        {
            return _settingName;
        }
    }
}