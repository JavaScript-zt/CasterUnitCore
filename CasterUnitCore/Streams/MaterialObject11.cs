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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Windows;
using CAPEOPEN;

namespace CasterUnitCore
{
    /// <summary>
    /// CO1.0 material wrapper
    /// </summary>
    [Serializable]
    public class MaterialObject11 : MaterialObject
    {
        #region Interface of material

        ICapeThermoMaterial _capeThermoMaterial;
        ICapeThermoMaterialContext _capeThermoMaterialContext;
        ICapeThermoPhases _capeThermoPhases;
        ICapeThermoCompounds _capeThermoCompounds;
        ICapeThermoPropertyRoutine _capeThermoPropertyRoutine;
        ICapeThermoEquilibriumRoutine _capeThermoEquilibriumRoutine;
        ICapeThermoUniversalConstant _capeThermoUniversalConstant;

        #endregion

        #region Constructor
        /// <summary>
        /// create a MaterialObject11
        /// </summary>
        public MaterialObject11()
            : this(null)
        {
        }
        /// <summary>
        /// create a MaterialObject11 connected to object, should only be invoked by CapeUnitPortBase
        /// </summary>
        public MaterialObject11(object objectToConnect)
        {
            SetMaterial(objectToConnect);
        }

        #endregion

        #region Material Object Manipulate

        public override object CapeThermoMaterialObject
        {
            get { return _capeThermoMaterial; }
        }

        public override void ClearAllProperties()
        {
            Debug.Assert(IsValid());
            _capeThermoMaterial.ClearAllProps();
            //_alreadyFlashed = false;
        }

        public override MaterialObject Duplicate()
        {
            Debug.Assert(IsValid());
            ICapeThermoMaterial newMaterial = _capeThermoMaterial.CreateMaterial();
            object sourceMaterial = _capeThermoMaterial;
            newMaterial.CopyFromMaterial(ref sourceMaterial);
            MaterialObject11 mo = new MaterialObject11();
            mo.SetMaterial(newMaterial);
            return mo;
        }

        public override bool IsValid()
        {
            return _capeThermoMaterial != null;
        }

        public override bool SetMaterial(object material)
        {
            if (material == null) return false;
            if (material is ICapeThermoMaterial)
            {
                //_alreadyFlashed = true;
                MaterialObjectVersion = 11;
                _capeThermoMaterial = material as ICapeThermoMaterial;
                _capeThermoMaterialContext = material as ICapeThermoMaterialContext;
                _capeThermoPhases = material as ICapeThermoPhases;
                _capeThermoCompounds = material as ICapeThermoCompounds;
                _capeThermoPropertyRoutine = material as ICapeThermoPropertyRoutine;
                _capeThermoEquilibriumRoutine = material as ICapeThermoEquilibriumRoutine;
                _capeThermoUniversalConstant = material as ICapeThermoUniversalConstant;
            }
            else if (material is MaterialObject11)
            {
                SetMaterial(((MaterialObject11)material).CapeThermoMaterialObject);
            }
            else
                throw new ArgumentException("parameter is not a CO1.1 material object");

            //Set Proper Phase name
            if (AllowedPhases.FirstOrDefault(phase => phase.Value.Contains("vap")) != null)
                Phases.Vapor = AllowedPhases.First(phase => phase.Value.Contains("vap"));
            if (AllowedPhases.FirstOrDefault(phase => phase.Value.Contains("liq")) != null)
                Phases.Liquid = AllowedPhases.First(phase => phase.Value.Contains("liq"));
            if (AllowedPhases.FirstOrDefault(phase => phase.Value.Contains("solid")) != null)
                Phases.Liquid = AllowedPhases.First(phase => phase.Value.Contains("solid"));
            UpdateCompoundList();
            return true;
        }

        public override void Dispose()
        {
            if (_capeThermoMaterial != null && _capeThermoMaterial.GetType().IsCOMObject)
                Marshal.FinalReleaseComObject(_capeThermoMaterial);
            if (_capeThermoCompounds != null && _capeThermoCompounds.GetType().IsCOMObject)
                Marshal.FinalReleaseComObject(_capeThermoCompounds);
            if (_capeThermoEquilibriumRoutine != null && _capeThermoEquilibriumRoutine.GetType().IsCOMObject)
                Marshal.FinalReleaseComObject(_capeThermoEquilibriumRoutine);
            if (_capeThermoMaterialContext != null && _capeThermoMaterialContext.GetType().IsCOMObject)
                Marshal.FinalReleaseComObject(_capeThermoMaterialContext);
            if (_capeThermoPhases != null && _capeThermoPhases.GetType().IsCOMObject)
                Marshal.FinalReleaseComObject(_capeThermoPhases);
            if (_capeThermoPropertyRoutine != null && _capeThermoPropertyRoutine.GetType().IsCOMObject)
                Marshal.FinalReleaseComObject(_capeThermoPropertyRoutine);
            if (_capeThermoUniversalConstant != null && _capeThermoUniversalConstant.GetType().IsCOMObject)
                Marshal.FinalReleaseComObject(_capeThermoUniversalConstant);
            _capeThermoMaterial = null;
            _capeThermoMaterialContext = null;
            _capeThermoPhases = null;
            _capeThermoCompounds = null;
            _capeThermoPropertyRoutine = null;
            _capeThermoEquilibriumRoutine = null;
            _capeThermoUniversalConstant = null;
        }

        #endregion

        #region DoFlash

        protected override bool CheckEquilibriumSpec(string[] flashSpec1, string[] flashSpec2, string solutionType)
        {
            return _capeThermoEquilibriumRoutine.CheckEquilibriumSpec(flashSpec1, flashSpec2, solutionType);
        }

        public override bool DoFlash(string[] flashSpec1, string[] flashSpec2, string solutionType, bool showWarning = false)
        {
            try
            {
                PresentPhases = AllowedPhases;
                _capeThermoEquilibriumRoutine.CalcEquilibrium(flashSpec1, flashSpec2, solutionType);
                //_alreadyFlashed = true;
            }
            catch (Exception e)
            {
                if (showWarning)
                    MessageBox.Show("Flash fails. " + e.Message);
                Debug.WriteLine("Flash fails. {0}", e.Message);
                return false;
            }
            return true;
        }

        public override bool DoTPFlash(bool showWarning = false)
        {
            if (_capeThermoEquilibriumRoutine == null) return false;
            string[] flashSpec1 = { "temperature", "", "Overall" };
            string[] flashSpec2 = { "pressure", "", "Overall" };
            try
            {
                PresentPhases = AllowedPhases;
                _capeThermoEquilibriumRoutine.CalcEquilibrium(flashSpec1, flashSpec2, "unspecified");
                //_alreadyFlashed = true;
            }
            catch (Exception e)
            {
                if (showWarning)
                    MessageBox.Show("Flash fails. " + e.Message);
                Debug.WriteLine("Flash fails. {0}", e.Message);
                return false;
            }
            return true;
        }

        public override bool DoPHFlash(bool showWarning = false)
        {
            if (_capeThermoEquilibriumRoutine == null) return false;
            string[] flashSpec1 = { "enthalpy", "", "Overall" };
            string[] flashSpec2 = { "pressure", "", "Overall" };
            try
            {
                PresentPhases = AllowedPhases;
                _capeThermoEquilibriumRoutine.CalcEquilibrium(flashSpec1, flashSpec2, "unspecified");
                //_alreadyFlashed = true;
            }
            catch (Exception e)
            {
                if (showWarning)
                    MessageBox.Show("Flash fails. " + e.Message);
                Debug.WriteLine("Flash fails. {0}", e.Message);
                return false;
            }
            return true;
        }

        public override bool DoTHFlash(bool showWarning = false)
        {
            if (_capeThermoEquilibriumRoutine == null) return false;
            string[] flashSpec1 = { "temperature", null, "Overall" };
            string[] flashSpec2 = { "enthalpy", null, "Overall" };
            try
            {
                PresentPhases = AllowedPhases;
                _capeThermoEquilibriumRoutine.CalcEquilibrium(flashSpec1, flashSpec2, "unspecified");
                //_alreadyFlashed = true;
            }
            catch (Exception e)
            {
                if (showWarning)
                    MessageBox.Show("Flash fails. " + e.Message);
                Debug.WriteLine("Flash fails. {0}", e.Message);
                return false;
            }
            return true;
        }

        public override bool DoTVFFlash(bool showWarning = false)
        {
            if (_capeThermoEquilibriumRoutine == null) return false;
            string[] flashSpec1 = { "temperature", null, "Overall" };
            string[] flashSpec2 = { "phaseFraction", "Mole", "gas" };
            try
            {
                PresentPhases = AllowedPhases;
                _capeThermoEquilibriumRoutine.CalcEquilibrium(flashSpec1, flashSpec2, "unspecified");
                //_alreadyFlashed = true;
            }
            catch (Exception e)
            {
                if (showWarning)
                    MessageBox.Show("Flash fails. " + e.Message);
                Debug.WriteLine("Flash fails. {0}", e.Message);
                return false;
            }
            return true;
        }

        public override bool DoPVFlash(bool showWarning = false)
        {
            if (_capeThermoEquilibriumRoutine == null) return false;
            string[] flashSpec1 = { "pressure", null, "Overall" };
            string[] flashSpec2 = { "phaseFraction", "Mole", "gas" };
            try
            {
                PresentPhases = AllowedPhases;
                _capeThermoEquilibriumRoutine.CalcEquilibrium(flashSpec1, flashSpec2, "unspecified");
                //_alreadyFlashed = true;
            }
            catch (Exception e)
            {
                if (showWarning)
                    MessageBox.Show("Flash fails. " + e.Message);
                Debug.WriteLine("Flash fails. {0}", e.Message);
                return false;
            }
            return true;
        }

        #endregion

        #region Phase

        public override Phases[] GetListOfAllowedPhase(out string[] phaseAggregationList, out string keyCompoundId)
        {
            object allowedPhaseObject = null;
            object phaseAggregationObject = null;
            object keyCompoundIdObject = null;
            phaseAggregationList = null;
            keyCompoundId = null;
            try
            {
                _capeThermoPhases.GetPhaseList(ref allowedPhaseObject, ref phaseAggregationObject, ref keyCompoundIdObject);
            }
            catch (Exception e) { Debug.WriteLine("Cannot get AllowedPhase. {0}", e.Message); }
            if (allowedPhaseObject == null)
                return new[] { Phases.Vapor, Phases.Liquid };

            phaseAggregationList = phaseAggregationObject as string[];
            keyCompoundId = keyCompoundIdObject as string;

            string[] phaseStringList = allowedPhaseObject as string[];
            Phases[] phaseList = (from phaseString in phaseStringList
                                  select new Phases(phaseString)).ToArray();
            return phaseList;
        }

        public override Phases[] GetListOfPresentPhases(out eCapePhaseStatus[] presentPhaseStatus)
        {
            object phaseLabel = null;
            object phaseStatus = null;
            presentPhaseStatus = null;
            if (_capeThermoMaterial == null) return null;
            _capeThermoMaterial.GetPresentPhases(ref phaseLabel, ref phaseStatus);
            string[] status = phaseStatus as string[];
            if (status != null)
            {
                presentPhaseStatus = new eCapePhaseStatus[status.Length];
                for (int i = 0; i < status.Length; i++)
                {
                    eCapePhaseStatus s;
                    if (Enum.TryParse(status[i], out s))
                        presentPhaseStatus[i] = s;
                }
            }

            string[] phaseStringList = phaseLabel as string[];
            Phases[] phaseList = (from phaseString in phaseStringList select new Phases(phaseString)).ToArray();
            return phaseList;
        }
        public override void SetListOfPresentPhases(IEnumerable<Phases> presentPhases, IEnumerable<eCapePhaseStatus> presentPhasesStatus)
        {
            if (_capeThermoMaterial == null) return;
            int[] phaseStatus = (from status in presentPhasesStatus select (int)status).ToArray();
            string[] phaseStringList = (from phase in presentPhases select phase.Value).ToArray();
            _capeThermoMaterial.SetPresentPhases(phaseStringList, phaseStatus);
            //_alreadyFlashed = false;
        }

        #endregion

        #region CompoundId

        public override string[] Formulas
        {
            get
            {
                object compoundId = null, formulaId = null, names = null, boilTemps = null, molwts = null, casnos = null;
                _capeThermoCompounds.GetCompoundList(ref compoundId, ref formulaId, ref names, ref boilTemps, ref molwts, ref casnos);
                return formulaId as string[];
            }
        }

        public override string[] UpdateCompoundList()
        {
            object compoundId = null, formulaId = null, names = null, boilTemps = null, molwts = null, casnos = null;
            try
            {
                _capeThermoCompounds.GetCompoundList(ref compoundId, ref formulaId,
                    ref names, ref boilTemps, ref molwts, ref casnos);
            }
            catch(Exception e)
            {
                Debug.WriteLine("Unable to get compound list. Make sure to call UpdateComoundList after compound list changed. {0}", e.Message);
            }
            return aliasName = compoundId as string[];
        }

        #endregion

        #region Overall Property

        public override double[] GetOverallPropList(string propName, PropertyBasis basis)
        {
            object value = null;
            _capeThermoMaterial.GetOverallProp(propName, basis.ToString(), ref value);
            return value as double[];
        }

        public override void SetOverallPropList(string propName, PropertyBasis basis, IEnumerable<double> value)
        {
            double[] temp = value as double[] ?? value.ToArray();
            _capeThermoMaterial.SetOverallProp(propName, basis.ToString(), value);
            //_alreadyFlashed = false;
        }

        public override double T
        {
            get
            {
                object value = null;
                _capeThermoMaterial.GetOverallProp("temperature", PropertyBasis.Undefined.ToString(), ref value);
                return (value as double[]).SingleOrDefault();
            }
            set
            {
                SetOverallPropDouble("temperature", PropertyBasis.Undefined, value);
            }
        }

        public override double P
        {
            get
            {
                object value = null;
                _capeThermoMaterial.GetOverallProp("pressure", PropertyBasis.Undefined.ToString(), ref value);
                return (value as double[]).SingleOrDefault();
            }
            set
            {
                SetOverallPropDouble("pressure", PropertyBasis.Undefined, value);
            }
        }

        public override double TotalFlow
        {
            get
            {
                object value = null;
                _capeThermoMaterial.GetOverallProp("totalFlow", PropertyBasis.Mole.ToString(), ref value);
                return (value as double[]).SingleOrDefault();
            }
            set
            {
                SetOverallPropDouble("totalFlow", PropertyBasis.Mole, value);
            }
        }

        public override double VaporFraction
        {
            get
            {
                object value = null;
                _capeThermoMaterial.GetSinglePhaseProp("phaseFraction", Phases.Vapor.Value, PropertyBasis.Mole.ToString(), ref value);
                double[] array = value as double[];
                if (array == null)
                    return 0;
                return array.SingleOrDefault();
            }
            set
            {
                var vaporExist = PresentPhases.Contains(Phases.Vapor);
                if (vaporExist == false)
                {
                    PresentPhases = AllowedPhases;
                }
                SetSinglePhasePropDouble("phaseFraction", Phases.Vapor, PropertyBasis.Mole, value);
            }
        }

        public override Dictionary<string, double> Composition
        {
            get
            {
                var composition = new Dictionary<string, double>();
                var compoundList = Compounds;
                object value = null;
                _capeThermoMaterial.GetOverallProp("fraction", PropertyBasis.Mole.ToString(), ref value);
                double[] compositionList = value as double[];
                for (int i = 0; i < compoundList.Length; i++)
                {
                    composition.Add(compoundList[i], compositionList[i]);
                }
                return composition;
            }
            set
            {
                double[] composition = new double[CompoundNum];
                for (int i = 0; i < CompoundNum; i++)
                {
                    value.TryGetValue(Compounds[i], out composition[i]);
                }
                SetOverallPropList("fraction", PropertyBasis.Mole, composition);
            }
        }

        #endregion

        #region Single Phase Property

        public override string[] AvailableSinglePhaseProp
        {
            get { return _capeThermoPropertyRoutine.GetSinglePhasePropList() as string[]; }
        }

        public override double[] GetSinglePhasePropList(string propName, Phases phase, PropertyBasis basis, bool calculate = true)
        {
            object value = null;
            if (PresentPhases.All(p => p.Value != phase.Value))
                return new double[CompoundNum];     //default is 0 for every element
            try
            {
                if (calculate)
                    _capeThermoPropertyRoutine.CalcSinglePhaseProp(new[] { propName }, phase.Value);
            }
            catch (Exception e)
            {
                Debug.WriteLine("Calculate single phase prop {0} fails. {1}", propName, e.Message);
            }
            _capeThermoMaterial.GetSinglePhaseProp(propName, phase.Value, basis.ToString(), ref value);
            return value as double[];
        }

        public override void SetSinglePhasePropList(string propName, Phases phase, PropertyBasis basis, IEnumerable<double> value)
        {
            if (PresentPhases.All(p => p.Value != phase.Value))
                PresentPhases = AllowedPhases;
            double[] temp = value as double[] ?? value.ToArray();
            _capeThermoMaterial.SetSinglePhaseProp(propName, phase.Value, basis.ToString(), temp);
            //_alreadyFlashed = false;
        }

        #endregion

        #region Two Phase Property

        public override string[] AvailableTwoPhaseProp
        {
            get { return _capeThermoPropertyRoutine.GetTwoPhasePropList(); }
        }

        public override double[] GetTwoPhasePropList(string propName, Phases phase1, Phases phase2, PropertyBasis basis, bool calculate = true)
        {
            object value = null;

            if (PresentPhases.All(p => p.Value != phase1.Value)
                || PresentPhases.All(p => p.Value != phase2.Value))
                return new double[CompoundNum];

            //将phases复制一遍到phaseList
            string[] phaseList = { phase1.Value, phase2.Value };
            try
            {
                _capeThermoPropertyRoutine.CalcTwoPhaseProp(new[] { propName }, phaseList);
            }
            catch (Exception e)
            {
                Debug.WriteLine("Calculate two phase prop {0} fails. {1}", propName, e.Message);
            }
            _capeThermoMaterial.GetTwoPhaseProp(propName, phaseList, basis.ToString(), ref value);
            return value as double[];
        }

        public override void SetTwoPhasePropList(string propName, Phases phase1, Phases phase2, PropertyBasis basis, IEnumerable<double> value)
        {
            if (PresentPhases.All(p => p.Value != phase1.Value)
                || PresentPhases.All(p => p.Value != phase2.Value))
                PresentPhases = AllowedPhases;

            string[] phaseList = { phase1.Value, phase2.Value };
            double[] temp = value as double[] ?? value.ToArray();
            _capeThermoMaterial.SetTwoPhaseProp(propName, phaseList, basis.ToString(), temp);
            //_alreadyFlashed = false;
        }

        #endregion

        #region Constant Property & T-Dependent Property & P-Dependent Property

        public override string[] AvailableConstProp
        {
            get { return _capeThermoCompounds.GetConstPropList() as string[]; }
        }

        public override string[] AvailableTDependentProp
        {
            get { return _capeThermoCompounds.GetTDependentPropList() as string[]; }
        }

        public override string[] AvailablePDependentProp
        {
            get { return _capeThermoCompounds.GetPDependentPropList() as string[]; }
        }

        public override string[] AvailableUniversalConstProp
        {
            get { return _capeThermoUniversalConstant.GetUniversalConstantList() as string[]; }
        }

        public override double GetCompoundConstPropDouble(string propName, string compoundId)
        {
            object[] value = _capeThermoCompounds.GetCompoundConstant(new[] { propName }, new[] { compoundId });
            try
            {
                return Convert.ToDouble(value.SingleOrDefault());
            }
            catch (Exception) { }
            try
            {
                return (value.Single() as double[]).SingleOrDefault();
            }
            catch (Exception e) { Debug.WriteLine("Get compound constant prop fails. {0}", e.Message); }
            return 0;
        }

        public override double GetCompoundTDependentProp(string propName, string compoundId, double T)
        {
            object value = null;
            _capeThermoCompounds.GetTDependentProperty(new[] { propName }, T, new[] { compoundId }, ref value);
            return (value as double[]).SingleOrDefault();
        }

        public override double GetCompoundPDependentProp(string propName, string compoundId, double P)
        {
            object value = null;
            _capeThermoCompounds.GetPDependentProperty(new[] { propName }, P, new[] { compoundId }, ref value);
            return (value as double[]).SingleOrDefault();
        }

        public override double GetUniversalConstProp(string constantId)
        {
            object temp = _capeThermoUniversalConstant.GetUniversalConstant(constantId);
            return Convert.ToDouble(temp);
        }

        #endregion
    }
}
