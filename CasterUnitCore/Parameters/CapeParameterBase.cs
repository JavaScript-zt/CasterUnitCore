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
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using CasterUnitCore.Annotations;
using CAPEOPEN;

namespace CasterUnitCore
{
    /// <summary>
    /// Common base class of Parameters
    /// </summary>
    [Serializable]
    [ComVisible(true)]
    [Guid("A4F42270-9605-4EB2-BAB7-AF8B66188607")]
    [ComDefaultInterface(typeof(ICapeParameter))]
    public abstract class CapeParameterBase : CapeOpenBaseObject,
        ICapeParameter, ICapeParameterSpec
    {
        /// <summary>
        /// default name is "parameter", default type is CAPE_REAL
        /// </summary>
        protected CapeParameterBase()
            : this("parameter", CapeParamType.CAPE_REAL, CapeParamMode.CAPE_INPUT_OUTPUT, UnitCategoryEnum.Dimensionless)
        { }
        /// <summary>
        /// 
        /// </summary>
        protected CapeParameterBase(string name, CapeParamType type, CapeParamMode mode, UnitCategoryEnum unitCategory)
            : base(name, null, false)
        {
            ValStatus = CapeValidationStatus.CAPE_NOT_VALIDATED;
            //value = null;
            Mode = mode;
            Type = type;
            Dimensionality = Units.GetDimensionality(unitCategory);
        }

        #region ICapeParameter
        /// <summary>
        /// 
        /// </summary>
        public abstract bool Validate(ref string message);
        /// <summary>
        /// 
        /// </summary>
        public abstract bool Validate();
        /// <summary>
        /// 
        /// </summary>
        public abstract void Reset();
        /// <summary>
        /// 
        /// </summary>
        public object Specification { get { return this; } }
        /// <summary>
        /// 
        /// </summary>
        public abstract object value { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public CapeValidationStatus ValStatus { get; protected set; }
        /// <summary>
        /// mode of this parameter, in most case, Parameters contains input parameter, Results contains output parameter
        /// </summary>
        public CapeParamMode Mode { get; set; }
        #endregion

        #region ICapeParameterSpec
        /// <summary>
        /// type of this parameter
        /// </summary>
        public CapeParamType Type { get; protected set; }
        /// <summary>
        /// return physical dimension of this parameter, in most case it was get through Units class
        /// </summary>
        public object Dimensionality { get; protected set; }
        #endregion

        #region ICloneable

        public abstract override object Clone();

        #endregion
    }
}
