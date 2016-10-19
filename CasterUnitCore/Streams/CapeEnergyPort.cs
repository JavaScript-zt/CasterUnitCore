﻿/*Copyright 2016 Caster

* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
* 
*     http://www.apache.org/licenses/LICENSE-2.0
* 
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using CAPEOPEN;

namespace CasterUnitCore
{
    /// <summary>
    /// UnitPort connect to energy
    /// </summary>
    [Serializable]
    [ComVisible(true)]
    [Guid("E5D0D2E7-18E9-4FE4-A0DA-87B76D78A894")]
    [ComDefaultInterface(typeof(ICapeUnitPort))]
    public class CapeEnergyPort : CapeUnitPort, IEnumerable<ICapeParameter>
    {
        [NonSerialized]
        protected ICapeCollection paramCollection;
        /// <summary>
        /// Create a energy port, contains a ICapeCollection
        /// </summary>
        public CapeEnergyPort(string name, CapePortDirection portDirection, string description = null, bool canRename = false)
            : base(name, CapePortType.CAPE_ENERGY, portDirection, description, canRename)
        { }

        #region Override of CapeUnitPortBase

        /// <summary>
        /// connect to ICapeCollection
        /// </summary>
        public override void Connect(object objectToConnect)
        {
            Disconnect();
            if (objectToConnect is ICapeCollection)
            {
                paramCollection = objectToConnect as ICapeCollection;
            }
            else
                throw new ECapeBadArgumentException("object connected to energy port must be ICapeCollection", 0);
        }

        public override void Disconnect()
        {
            if (paramCollection != null && paramCollection.GetType().IsCOMObject)
                Marshal.FinalReleaseComObject(paramCollection);
            paramCollection = null;
        }

        /// <summary>
        /// get the raw COM interface of ICapeCollection created by simulator
        /// </summary>
        public override object connectedObject
        {
            get
            {
                return paramCollection;
            }
        }

        public override bool IsConnected()
        {
            return paramCollection != null;
        }


        public override object Clone()
        {
            return new CapeEnergyPort(ComponentName, _portDirection, ComponentDescription, CanRename)
            {
                paramCollection = paramCollection,
            };
        }

        #endregion

        #region Energy port
        /// <summary>
        /// Get Item by a string id or a int id
        /// </summary>
        public ICapeParameter this[string id]
        {
            get { return paramCollection.Item(id); }
        }
        /// <summary>
        /// Get Item by a string id or a int id
        /// </summary>
        public ICapeParameter this[int index]
        {         //collection start with 1
            get { return paramCollection.Item(index + 1); }
        }
        /// <summary>
        /// return the number in this collection
        /// </summary>
        /// <returns></returns>
        public int Count
        {
            get { return paramCollection.Count(); }
        }
        /// <summary>
        /// return work parameter in this energy port, if not present, return 0
        /// </summary>
        public double Work
        {
            get
            {
                return (from x in this
                        where ((ICapeIdentification)x).ComponentName.ToLower() == "work"
                        select x.value).SingleOrDefault();
            }
            set
            {
                int index = -1;
                for (int i = 0; i < Count; i++)
                    if (((ICapeIdentification)this[i]).ComponentName.ToLower() == "work")
                    {
                        index = i;
                        break;
                    }
                if (index != -1)
                    ((ICapeParameter)paramCollection.Item(index)).value = value;
                else
                    throw new ECapeInvalidOperationException("Energy port don't contain work parameter!");
            }
        }
        /// <summary>
        /// return temperatureLow parameter in this energy port, if not present, return 0
        /// </summary>
        public double TemperatureLow
        {
            get
            {
                return (from x in this
                        where ((ICapeIdentification)x).ComponentName.ToLower() == "temperaturelow"
                        select x.value).SingleOrDefault();
            }
            set
            {
                int index = -1;
                for (int i = 0; i < Count; i++)
                    if (((ICapeIdentification)this[i]).ComponentName.ToLower() == "temperaturelow")
                    {
                        index = i;
                        break;
                    }
                if (index != -1)
                    ((ICapeParameter)paramCollection.Item(index)).value = value;
                else
                    throw new ECapeInvalidOperationException("Energy port don't contain temperaturelow parameter!");
            }
        }
        /// <summary>
        /// return temperatureHigh parameter in this energy port, if not present, return 0
        /// </summary>
        public double TemperatureHigh
        {
            get
            {
                return (from x in this
                        where ((ICapeIdentification)x).ComponentName.ToLower() == "temperaturehigh"
                        select x.value).SingleOrDefault();
            }
            set
            {
                int index = -1;
                for (int i = 0; i < Count; i++)
                    if (((ICapeIdentification)this[i]).ComponentName.ToLower() == "temperaturehigh")
                    {
                        index = i;
                        break;
                    }
                if (index != -1)
                    ((ICapeParameter)paramCollection.Item(index)).value = value;
                else
                    throw new ECapeInvalidOperationException("Energy port don't contain temperaturehigh parameter!");
            }
        }

        #endregion


        #region Implementation of IEnumerable

        public IEnumerator<ICapeParameter> GetEnumerator()
        {
            List<ICapeParameter> items = new List<ICapeParameter>(Count);
            for (int i = 0; i < Count; i++)
                items.Add(this[i]);
            return items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
