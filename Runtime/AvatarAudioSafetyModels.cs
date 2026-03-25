using System;
using System.Collections.Generic;
using UnityEngine;

namespace Sebanne.AvatarAudioSafetyGuard
{
    public enum AvatarAudioSafetyMode
    {
        PreviewOnly = 0,
        ApplyOnBuild = 1,
    }

    public enum AvatarAudioSafetyProfile
    {
        Conservative = 0,
        Standard = 1,
        Custom = 2,
    }

    public enum AvatarAudioSafetyRule
    {
        Default = 0,
        Ignore = 1,
        ReportOnly = 2,
        CustomThreshold = 3,
    }

    public enum AvatarAudioSafetyResultKind
    {
        Safe = 0,
        Warning = 1,
        WouldClamp = 2,
        ReportOnly = 3,
        Ignored = 4,
        ManualReview = 5,
    }

    public enum AvatarAudioSafetyBuildResultEntryStatus
    {
        Changed = 0,
        Skipped = 1,
        Unchanged = 2,
        Error = 3,
    }

    [Serializable]
    public sealed class AvatarAudioThresholdPreset
    {
        [Min(0f)]
        public float maxGain = 10f;

        [Min(0f)]
        public float maxFarDistance = 10f;

        [Range(0f, 1f)]
        public float maxVolume = 1f;

        public AvatarAudioThresholdPreset Clone()
        {
            return new AvatarAudioThresholdPreset
            {
                maxGain = maxGain,
                maxFarDistance = maxFarDistance,
                maxVolume = maxVolume,
            };
        }

        public void CopyFrom(AvatarAudioThresholdPreset other)
        {
            if (other == null)
            {
                return;
            }

            maxGain = other.maxGain;
            maxFarDistance = other.maxFarDistance;
            maxVolume = other.maxVolume;
        }
    }

    public static class AvatarAudioThresholdPresets
    {
        public static AvatarAudioThresholdPreset CreateConservative()
        {
            return new AvatarAudioThresholdPreset
            {
                maxGain = 6f,
                maxFarDistance = 6f,
                maxVolume = 0.75f,
            };
        }

        public static AvatarAudioThresholdPreset CreateStandard()
        {
            return new AvatarAudioThresholdPreset
            {
                maxGain = 10f,
                maxFarDistance = 10f,
                maxVolume = 0.9f,
            };
        }

        public static AvatarAudioThresholdPreset CreateCustomDefault()
        {
            return CreateStandard();
        }

        public static AvatarAudioThresholdPreset Resolve(AvatarAudioSafetyProfile profile, AvatarAudioThresholdPreset customThresholds)
        {
            switch (profile)
            {
                case AvatarAudioSafetyProfile.Conservative:
                    return CreateConservative();
                case AvatarAudioSafetyProfile.Custom:
                    return customThresholds != null ? customThresholds.Clone() : CreateCustomDefault();
                case AvatarAudioSafetyProfile.Standard:
                default:
                    return CreateStandard();
            }
        }
    }

    [Serializable]
    public sealed class AvatarAudioSourceRuleEntry
    {
        public string path = string.Empty;
        public AvatarAudioSafetyRule rule = AvatarAudioSafetyRule.Default;
        public AvatarAudioThresholdPreset customThresholds = AvatarAudioThresholdPresets.CreateCustomDefault();

        [TextArea(2, 4)]
        public string memo = string.Empty;
    }

    [Serializable]
    public sealed class AvatarAudioScanResult
    {
        public AvatarAudioSafetyResultKind result = AvatarAudioSafetyResultKind.Safe;
        public string path = string.Empty;
        public AudioSource audioSource;
        public AudioClip clip;
        public bool loop;

        [Range(0f, 1f)]
        public float volume = 1f;

        public float gain;
        public float farDistance = 10f;
        public float nearDistance;
        public float volumetricRadius;
        public AvatarAudioSafetyRule appliedRule = AvatarAudioSafetyRule.Default;
        public string reason = string.Empty;
        public string plannedChange = string.Empty;

        public AvatarAudioScanResult Clone()
        {
            return new AvatarAudioScanResult
            {
                result = result,
                path = path,
                audioSource = audioSource,
                clip = clip,
                loop = loop,
                volume = volume,
                gain = gain,
                farDistance = farDistance,
                nearDistance = nearDistance,
                volumetricRadius = volumetricRadius,
                appliedRule = appliedRule,
                reason = reason,
                plannedChange = plannedChange,
            };
        }
    }

    [Serializable]
    public sealed class AvatarAudioScanSummary
    {
        public int scanned;
        public int safe;
        public int warnings;
        public int wouldClamp;
        public int reportOnly;
        public int ignored;
        public int manualReview;
        public string lastScanLocalTime = string.Empty;

        public void Reset()
        {
            scanned = 0;
            safe = 0;
            warnings = 0;
            wouldClamp = 0;
            reportOnly = 0;
            ignored = 0;
            manualReview = 0;
            lastScanLocalTime = string.Empty;
        }

        public void UpdateFromResults(IReadOnlyList<AvatarAudioScanResult> results)
        {
            Reset();

            if (results == null)
            {
                return;
            }

            scanned = results.Count;

            for (int i = 0; i < results.Count; i++)
            {
                AvatarAudioScanResult result = results[i];
                if (result == null)
                {
                    continue;
                }

                switch (result.result)
                {
                    case AvatarAudioSafetyResultKind.Warning:
                        warnings++;
                        break;
                    case AvatarAudioSafetyResultKind.WouldClamp:
                        wouldClamp++;
                        break;
                    case AvatarAudioSafetyResultKind.ReportOnly:
                        reportOnly++;
                        break;
                    case AvatarAudioSafetyResultKind.Ignored:
                        ignored++;
                        break;
                    case AvatarAudioSafetyResultKind.ManualReview:
                        manualReview++;
                        break;
                    case AvatarAudioSafetyResultKind.Safe:
                    default:
                        safe++;
                        break;
                }
            }

            lastScanLocalTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }

        public AvatarAudioScanSummary Clone()
        {
            return new AvatarAudioScanSummary
            {
                scanned = scanned,
                safe = safe,
                warnings = warnings,
                wouldClamp = wouldClamp,
                reportOnly = reportOnly,
                ignored = ignored,
                manualReview = manualReview,
                lastScanLocalTime = lastScanLocalTime,
            };
        }

        public string ToDisplayString()
        {
            if (scanned <= 0)
            {
                return "No AudioSource scanned yet.";
            }

            return string.Format(
                "scanned: {0} | safe: {1} | warnings: {2} | would clamp: {3} | report only: {4} | ignored: {5} | manual review: {6}",
                scanned,
                safe,
                warnings,
                wouldClamp,
                reportOnly,
                ignored,
                manualReview);
        }
    }

    [Serializable]
    public sealed class AvatarAudioSafetyBuildResultEntry
    {
        public AvatarAudioSafetyBuildResultEntryStatus status = AvatarAudioSafetyBuildResultEntryStatus.Unchanged;
        public AvatarAudioSafetyResultKind evaluationResult = AvatarAudioSafetyResultKind.Safe;
        public AvatarAudioSafetyRule appliedRule = AvatarAudioSafetyRule.Default;
        public string path = string.Empty;
        public string clipName = string.Empty;
        public string reason = string.Empty;
        public string detail = string.Empty;
        public string beforeSummary = string.Empty;
        public string afterSummary = string.Empty;

        public AvatarAudioSafetyBuildResultEntry Clone()
        {
            return new AvatarAudioSafetyBuildResultEntry
            {
                status = status,
                evaluationResult = evaluationResult,
                appliedRule = appliedRule,
                path = path,
                clipName = clipName,
                reason = reason,
                detail = detail,
                beforeSummary = beforeSummary,
                afterSummary = afterSummary,
            };
        }
    }

    [Serializable]
    public sealed class AvatarAudioSafetyBuildResultSnapshot
    {
        public string executedLocalTime = string.Empty;
        public string avatarName = string.Empty;
        public AvatarAudioSafetyMode mode = AvatarAudioSafetyMode.PreviewOnly;
        public string summaryMessage = string.Empty;
        public int scanned;
        public int changed;
        public int skipped;
        public int unchanged;
        public int errors;
        public List<AvatarAudioSafetyBuildResultEntry> entries = new List<AvatarAudioSafetyBuildResultEntry>();

        public bool HasData
        {
            get
            {
                return !string.IsNullOrEmpty(executedLocalTime)
                    || !string.IsNullOrEmpty(summaryMessage)
                    || scanned > 0
                    || changed > 0
                    || skipped > 0
                    || unchanged > 0
                    || errors > 0
                    || (entries != null && entries.Count > 0);
            }
        }

        public void Reset()
        {
            executedLocalTime = string.Empty;
            avatarName = string.Empty;
            mode = AvatarAudioSafetyMode.PreviewOnly;
            summaryMessage = string.Empty;
            scanned = 0;
            changed = 0;
            skipped = 0;
            unchanged = 0;
            errors = 0;

            if (entries == null)
            {
                entries = new List<AvatarAudioSafetyBuildResultEntry>();
            }
            else
            {
                entries.Clear();
            }
        }

        public AvatarAudioSafetyBuildResultSnapshot Clone()
        {
            AvatarAudioSafetyBuildResultSnapshot clone = new AvatarAudioSafetyBuildResultSnapshot
            {
                executedLocalTime = executedLocalTime,
                avatarName = avatarName,
                mode = mode,
                summaryMessage = summaryMessage,
                scanned = scanned,
                changed = changed,
                skipped = skipped,
                unchanged = unchanged,
                errors = errors,
            };

            if (entries != null)
            {
                for (int i = 0; i < entries.Count; i++)
                {
                    AvatarAudioSafetyBuildResultEntry entry = entries[i];
                    if (entry != null)
                    {
                        clone.entries.Add(entry.Clone());
                    }
                }
            }

            return clone;
        }
    }
}
