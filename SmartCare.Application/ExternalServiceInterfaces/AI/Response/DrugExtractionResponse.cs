using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.ExternalServiceInterfaces.AI.Response
{
    using System.Text.Json.Serialization;

    using System.Text.Json.Serialization;

    public record DetectionItem(
        [property: JsonPropertyName("bbox")] List<int> BBox,
        [property: JsonPropertyName("confidence")] float Confidence
    );

    public record DrugExtractionResponse(
        [property: JsonPropertyName("detections")] List<DetectionItem> Detections,

        [property: JsonPropertyName("active_ingredients")]
    List<string> ActiveIngredients
    );
}
