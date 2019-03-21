/**
 * This sample script sets the field order of all media with a video
 * stream to progressive scan.
 *
 * Revision Date: Oct. 23, 2006
 **/
using Sony.Vegas;

public class EntryPoint {
    public void FromVegas(Vegas vegas) {
        // Set the following variable to the VideoFieldOrder value you
        // wish to have all video streams use.
        VideoFieldOrder targetFieldOrder = VideoFieldOrder.ProgressiveScan;
        foreach (Media media in vegas.Project.MediaPool) {
            if (media.HasVideo()) {
                VideoStream videoStream = (VideoStream) media.Streams.GetItemByMediaType(MediaType.Video, 0);
                videoStream.FieldOrder = targetFieldOrder;
            }
        }
    }
}
