using System.Collections.Generic;

namespace Modular2DCharacterController.Runtime.Data.FeatureProfiles
{
    /// <summary>
    /// Provides the functionalities to register and unregister a feature profile, and retrieve the current one
    /// with the highest priority.
    /// </summary>
    public class ProfileProvider<TProfile> where TProfile : FeatureProfile
    {
        private readonly List<TProfile> _profiles = new();
        private TProfile _currentProfile;

        public void RegisterProfile(TProfile profile)
        {
            if (profile == null)
                return;

            if (_profiles.Contains(profile))
                return;

            _profiles.Add(profile);
            UpdateCurrentProfile();
        }

        public void UnregisterProfile(TProfile profile)
        {
            if (profile == null)
                return;

            _profiles.Remove(profile);
            UpdateCurrentProfile();
        }

        // Keeps track of what is the highest priority profile. Updated whenever a profile is registered
        // or unregistered.
        private void UpdateCurrentProfile()
        {
            if (_profiles.Count == 0)
            {
                _currentProfile = null;
                return;
            }

            TProfile highestPriorityProfile = _profiles[0];

            for (int i = 1; i < _profiles.Count; i++)
            {
                if (_profiles[i].priority > highestPriorityProfile.priority)
                {
                    highestPriorityProfile = _profiles[i];
                }
            }

            _currentProfile = highestPriorityProfile;
        }

        public TProfile GetCurrentProfile()
        {
            return _currentProfile;
        }
    }
}