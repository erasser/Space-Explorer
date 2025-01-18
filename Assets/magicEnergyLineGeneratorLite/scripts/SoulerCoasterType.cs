using UnityEngine;

namespace soulercoasterLite.scripts {
    public enum SoulerCoasterType {
        [Tooltip("Generates 1 plane in horizontal space")]
        HORZONTAL_QUAD,

        [Tooltip("Generates 1 plane in vertical space")]
        VERTICAL_QUAD,

        [Tooltip("Generates 2 planes orthogonal on each other")]
        SOULERCOASTER,

        [Tooltip("Generates a mesh consisting o n planes arranged in a polygon.")]
        SOULERCOASTER_N_GON,

        [Tooltip("Generates a mesh according to your CustomPlaneConfiguration.")]
        CUSTOM_PLANE_CONFIGURATION
    }
}