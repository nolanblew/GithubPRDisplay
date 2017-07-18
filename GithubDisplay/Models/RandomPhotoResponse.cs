using System;
using Newtonsoft.Json;

namespace GithubDisplay.Models
{
    public class RandomPhotoResponse
    {
        public string id { get; set; }
        public DateTime? created_at { get; set; }
        public DateTime? updated_at { get; set; }
        public int? width { get; set; }
        public int? height { get; set; }
        public string color { get; set; }
        public object slug { get; set; }
        public int? downloads { get; set; }
        public int? likes { get; set; }
        public int? views { get; set; }
        public bool? liked_by_user { get; set; }
        public Exif exif { get; set; }
        public Location location { get; set; }
        public object[] current_user_collections { get; set; }
        public Urls urls { get; set; }
        public Category[] categories { get; set; }
        public Links links { get; set; }
        public User user { get; set; }
    }

    public class Exif
    {
        public string make { get; set; }
        public string model { get; set; }
        public string exposure_time { get; set; }
        public string aperture { get; set; }
        public string focal_length { get; set; }
        public int? iso { get; set; }
    }

    public class Location
    {
        public string title { get; set; }
        public string name { get; set; }
        public string city { get; set; }
        public string country { get; set; }
        public Position position { get; set; }
    }

    public class Position
    {
        public float? latitude { get; set; }
        public float? longitude { get; set; }
    }

    public class Urls
    {
        public string raw { get; set; }
        public string full { get; set; }
        public string regular { get; set; }
        public string small { get; set; }
        public string thumb { get; set; }
    }

    public class Links
    {
        public string self { get; set; }
        public string html { get; set; }
        public string download { get; set; }
        public string download_location { get; set; }
    }

    public class User
    {
        public string id { get; set; }
        public DateTime? updated_at { get; set; }
        public string username { get; set; }
        public string name { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string portfolio_url { get; set; }
        public string bio { get; set; }
        public string location { get; set; }
        public int? total_likes { get; set; }
        public int? total_photos { get; set; }
        public int? total_collections { get; set; }
        public Profile_Image profile_image { get; set; }
        public Links1 links { get; set; }
    }

    public class Profile_Image
    {
        public string small { get; set; }
        public string medium { get; set; }
        public string large { get; set; }
    }

    public class Links1
    {
        public string self { get; set; }
        public string html { get; set; }
        public string photos { get; set; }
        public string likes { get; set; }
        public string portfolio { get; set; }
        public string following { get; set; }
        public string followers { get; set; }
    }

    public class Category
    {
        public int? id { get; set; }
        public string title { get; set; }
        public int? photo_count { get; set; }
        public Links2 links { get; set; }
    }

    public class Links2
    {
        public string self { get; set; }
        public string photos { get; set; }
    }

}
