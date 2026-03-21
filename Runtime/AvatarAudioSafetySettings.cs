using System.Collections.Generic;
using UnityEngine;

namespace Sebanne.AvatarAudioSafetyGuard
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Avatar Audio Safety Guard/Avatar Audio Safety Settings")]
    public sealed class AvatarAudioSafetySettings : MonoBehaviour
    {
        [SerializeField]
        private bool toolEnabled = true;

        [SerializeField]
        private AvatarAudioSafetyMode mode = AvatarAudioSafetyMode.PreviewOnly;

        [SerializeField]
        private AvatarAudioSafetyProfile profile = AvatarAudioSafetyProfile.Standard;

        [SerializeField]
        private AvatarAudioThresholdPreset customThresholds = AvatarAudioThresholdPresets.CreateCustomDefault();

        [SerializeField]
        private bool warnOnMissingVrcSpatialAudioSource = true;

        [SerializeField]
        private bool warnOnCustomRolloff = true;

        [SerializeField]
        private bool warnOnLoopWithLongRange = true;

        [SerializeField]
        private bool warnOnNon3DAudio = true;

        [SerializeField]
        private List<AvatarAudioSourceRuleEntry> perSourceRules = new List<AvatarAudioSourceRuleEntry>();

        [SerializeField]
        private AvatarAudioScanSummary lastScanSummary = new AvatarAudioScanSummary();

        [SerializeField]
        private List<AvatarAudioScanResult> detectedAudioSources = new List<AvatarAudioScanResult>();

        public bool ToolEnabled
        {
            get { return toolEnabled; }
        }

        public AvatarAudioSafetyMode Mode
        {
            get { return mode; }
        }

        public AvatarAudioSafetyProfile Profile
        {
            get { return profile; }
        }

        public bool ShouldApplyOnBuild
        {
            get { return toolEnabled && mode == AvatarAudioSafetyMode.ApplyOnBuild; }
        }

        public AvatarAudioThresholdPreset CustomThresholds
        {
            get { return customThresholds; }
        }

        public bool WarnOnMissingVrcSpatialAudioSource
        {
            get { return warnOnMissingVrcSpatialAudioSource; }
        }

        public bool WarnOnCustomRolloff
        {
            get { return warnOnCustomRolloff; }
        }

        public bool WarnOnLoopWithLongRange
        {
            get { return warnOnLoopWithLongRange; }
        }

        public bool WarnOnNon3DAudio
        {
            get { return warnOnNon3DAudio; }
        }

        public IReadOnlyList<AvatarAudioSourceRuleEntry> PerSourceRules
        {
            get { return perSourceRules; }
        }

        public AvatarAudioScanSummary LastScanSummary
        {
            get { return lastScanSummary; }
        }

        public IReadOnlyList<AvatarAudioScanResult> DetectedAudioSources
        {
            get { return detectedAudioSources; }
        }

        public AvatarAudioThresholdPreset ResolveDefaultThresholds()
        {
            EnsureDefaults();
            return AvatarAudioThresholdPresets.Resolve(profile, customThresholds);
        }

        public void SetScanResults(IReadOnlyList<AvatarAudioScanResult> results, AvatarAudioScanSummary summary)
        {
            EnsureDefaults();

            detectedAudioSources.Clear();

            if (results != null)
            {
                for (int i = 0; i < results.Count; i++)
                {
                    AvatarAudioScanResult result = results[i];
                    if (result != null)
                    {
                        detectedAudioSources.Add(result.Clone());
                    }
                }
            }

            if (summary != null)
            {
                lastScanSummary = summary.Clone();
            }
            else
            {
                lastScanSummary.Reset();
            }
        }

        public void ClearScanResults()
        {
            EnsureDefaults();
            detectedAudioSources.Clear();
            lastScanSummary.Reset();
        }

        private void Reset()
        {
            EnsureDefaults();
        }

        private void OnValidate()
        {
            EnsureDefaults();
        }

        private void EnsureDefaults()
        {
            if (customThresholds == null)
            {
                customThresholds = AvatarAudioThresholdPresets.CreateCustomDefault();
            }

            if (perSourceRules == null)
            {
                perSourceRules = new List<AvatarAudioSourceRuleEntry>();
            }

            if (detectedAudioSources == null)
            {
                detectedAudioSources = new List<AvatarAudioScanResult>();
            }

            if (lastScanSummary == null)
            {
                lastScanSummary = new AvatarAudioScanSummary();
            }

            for (int i = 0; i < perSourceRules.Count; i++)
            {
                AvatarAudioSourceRuleEntry rule = perSourceRules[i];
                if (rule != null && rule.customThresholds == null)
                {
                    rule.customThresholds = AvatarAudioThresholdPresets.CreateCustomDefault();
                }
            }
        }
    }
}
