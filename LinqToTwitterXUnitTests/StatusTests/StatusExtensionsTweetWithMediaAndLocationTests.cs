﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using LinqToTwitter;
using Moq;
using Xunit;

namespace LinqToTwitterXUnitTests.StatusTests
{
    public class StatusExtensionsTweetsWithMediaAndLocationTests
    {
        readonly string expectedUploadUrl = "https://upload.twitter.com/1/statuses/update_with_media.xml";

        readonly Mock<TwitterContext> twitterCtx;
        readonly Mock<ITwitterExecute> twitterExecute;
        readonly Mock<IRequestProcessor<Status>> statusReqProc;
         
        readonly string frenchLocale = "FR-fr";

        string status = "test";
        bool possiblySensitive = true;
        decimal latitude = 37.78215m;
        decimal longitude = -122.40060m;
        string placeID = "place";
        bool displayCoordinates = true;
        ulong inReplyToStatusID = 23030327348932ul;

        List<Media> mediaItems = new List<Media> 
        { 
            new Media
            {
                Data = new byte[] { 0xFF },
                FileName = "AnImage.jpg",
                ContentType = MediaContentType.JPEG
            }
        };

        #region Test Data

        private readonly string testStatusQueryResponse = @"<statuses type=""array"">
    <status>
      <created_at>Fri Nov 27 18:28:57 +0000 2009</created_at>
      <id>6118906745</id>
      <text>ah,vou lá comer</text>
      <source>web</source>
      <truncated>false</truncated>
      <in_reply_to_status_id></in_reply_to_status_id>
      <in_reply_to_user_id></in_reply_to_user_id>
      <favorited>false</favorited>
      <in_reply_to_screen_name></in_reply_to_screen_name>
      <user>
        <id>77880019</id>
        <name>caah </name>
        <screen_name>caahbuss</screen_name>
        <location></location>
        <description></description>
        <profile_image_url>http://a1.twimg.com/profile_images/440024240/d_normal.JPG</profile_image_url>
        <url></url>
        <protected>false</protected>
        <followers_count>48</followers_count>
        <profile_background_color>131516</profile_background_color>
        <profile_text_color>333333</profile_text_color>
        <profile_link_color>009999</profile_link_color>
        <profile_sidebar_fill_color>efefef</profile_sidebar_fill_color>
        <profile_sidebar_border_color>eeeeee</profile_sidebar_border_color>
        <friends_count>47</friends_count>
        <created_at>Mon Sep 28 00:47:48 +0000 2009</created_at>
        <favourites_count>0</favourites_count>
        <utc_offset></utc_offset>
        <time_zone></time_zone>
        <profile_background_image_url>http://s.twimg.com/a/1259091217/images/themes/theme14/bg.gif</profile_background_image_url>
        <profile_background_tile>true</profile_background_tile>
        <statuses_count>211</statuses_count>
        <notifications>false</notifications>
        <geo_enabled>false</geo_enabled>
        <verified>false</verified>
        <following>false</following>
      </user>
      <geo />
    </status>
  </statuses>";

        #endregion

        public StatusExtensionsTweetsWithMediaAndLocationTests()
        {
            this.statusReqProc = new Mock<IRequestProcessor<Status>>();
            this.statusReqProc.Setup(reqProc => reqProc.ProcessResults(It.IsAny<string>()))
            .Returns(new List<Status> { new Status { Text = "Test" } });

            this.twitterExecute = new Mock<ITwitterExecute>();
            this.twitterExecute.Setup(exec => 
                exec.PostMedia(
                    It.IsAny<string>(),
                    It.IsAny<IDictionary<string, string>>(),
                    It.IsAny<List<Media>>(),
                    It.IsAny<IRequestProcessor<Status>>()))
            .Returns(this.testStatusQueryResponse);

            this.twitterCtx = new Mock<TwitterContext>(this.twitterExecute.Object);
            this.twitterCtx.Setup(ctx => ctx.CreateRequestProcessor<Status>())
            .Returns(this.statusReqProc.Object);
        }

        [Fact]
        public void TweetWithMedia_Throws_On_Null_Status()
        {
            this.status = null;

            ArgumentNullException ex =
            Assert.Throws<ArgumentNullException>(() =>
            {
                StatusExtensions.TweetWithMedia(
                    this.twitterCtx.Object, this.status, this.possiblySensitive,
                    this.latitude, this.longitude, this.placeID, this.displayCoordinates,
                    this.mediaItems, null);
            });

            Assert.Equal("status", ex.ParamName);
        }

        [Fact]
        public void TweetWithMedia_Calls_PostMedia()
        {
            StatusExtensions.TweetWithMedia(
                this.twitterCtx.Object, this.status, this.possiblySensitive,
                this.latitude, this.longitude, this.placeID, this.displayCoordinates,
                this.mediaItems, null);

            this.twitterExecute.Verify(exec =>
                                       exec.PostMedia(
                                           It.IsAny<string>(),
                                           It.IsAny<IDictionary<string, string>>(),
                                           It.IsAny<List<Media>>(),
                                           It.IsAny<IRequestProcessor<Status>>()),
                                       Times.Once(),
                                       "PostMedia was not called only one time.");
        }

        [Fact]
        public void TweetWithMedia_Passes_Properly_Formatted_Url_to_PostMedia()
        {
            StatusExtensions.TweetWithMedia(
                this.twitterCtx.Object, this.status, this.possiblySensitive,
                this.latitude, this.longitude, this.placeID, this.displayCoordinates,
                this.mediaItems, null);

            this.twitterExecute.Verify(exec =>
                                       exec.PostMedia(
                                           It.Is<string>(url => url == this.expectedUploadUrl),
                                           It.IsAny<IDictionary<string, string>>(),
                                           It.IsAny<List<Media>>(),
                                           It.IsAny<IRequestProcessor<Status>>()),
                                       Times.Once(),
                                       "TweetWithMedia didn't pass properly formatted URL.");
        }

        [Fact]
        public void TweetWithMedia_Passes_Status_via_Parameter_Dictionary_to_PostMedia()
        {
            StatusExtensions.TweetWithMedia(
                this.twitterCtx.Object, this.status, this.possiblySensitive,
                this.latitude, this.longitude, this.placeID, this.displayCoordinates,
                this.mediaItems, null);

            this.twitterExecute.Verify(exec =>
                                       exec.PostMedia(
                                           It.IsAny<string>(),
                                           It.Is<IDictionary<string, string>>(parms => parms["status"] == status),
                                           It.IsAny<List<Media>>(),
                                           It.IsAny<IRequestProcessor<Status>>()),
                                       Times.Once(),
                                       "TweetWithMedia didn't pass status properly.");
        }

        [Fact]
        public void TweetWithMedia_Passes_possiblySensitive_via_Parameter_Dictionary_to_PostMedia()
        {
            StatusExtensions.TweetWithMedia(
                this.twitterCtx.Object, this.status, this.possiblySensitive,
                this.latitude, this.longitude, this.placeID, this.displayCoordinates,
                this.mediaItems, null);

            this.twitterExecute.Verify(exec =>
                                       exec.PostMedia(
                                           It.IsAny<string>(),
                                           It.Is<IDictionary<string, string>>(parms => parms["possibly_sensitive"] == possiblySensitive.ToString()),
                                           It.IsAny<List<Media>>(),
                                           It.IsAny<IRequestProcessor<Status>>()),
                                       Times.Once(),
                                       "TweetWithMedia didn't pass possiblySensitive parameter properly.");
        }

        [Fact]
        public void TweetWithMedia_DoesNot_Pass_false_possiblySensitive_via_Parameter_Dictionary_to_PostMedia()
        {
            this.possiblySensitive = false;

            StatusExtensions.TweetWithMedia(
                this.twitterCtx.Object, this.status, this.possiblySensitive,
                this.latitude, this.longitude, this.placeID, this.displayCoordinates,
                this.mediaItems, null);

            this.twitterExecute.Verify(exec =>
                                       exec.PostMedia(
                                           It.IsAny<string>(),
                                           It.Is<IDictionary<string, string>>(parms => parms["possibly_sensitive"] == null),
                                           It.IsAny<List<Media>>(),
                                           It.IsAny<IRequestProcessor<Status>>()),
                                       Times.Once(),
                                       "TweetWithMedia should not have passed possiblySensitive parameter.");
        }

        [Fact]
        public void TweetWithMedia_Passes_placeID_via_Parameter_Dictionary_to_PostMedia()
        {
            StatusExtensions.TweetWithMedia(
                this.twitterCtx.Object, this.status, this.possiblySensitive,
                this.latitude, this.longitude, this.placeID, this.displayCoordinates,
                this.mediaItems, null);

            this.twitterExecute.Verify(exec =>
                                       exec.PostMedia(
                                           It.IsAny<string>(),
                                           It.Is<IDictionary<string, string>>(parms => parms["place_id"] == placeID),
                                           It.IsAny<List<Media>>(),
                                           It.IsAny<IRequestProcessor<Status>>()),
                                       Times.Once(),
                                       "TweetWithMedia didn't pass placeID parameter properly.");
        }

        [Fact]
        public void TweetWithMedia_DoesNot_Pass_null_placeID_via_Parameter_Dictionary_to_PostMedia()
        {
            this.placeID = null;

            StatusExtensions.TweetWithMedia(
                this.twitterCtx.Object, this.status, this.possiblySensitive,
                this.latitude, this.longitude, this.placeID, this.displayCoordinates,
                this.mediaItems, null);

            this.twitterExecute.Verify(exec =>
                                       exec.PostMedia(
                                           It.IsAny<string>(),
                                           It.Is<IDictionary<string, string>>(parms => parms["place_id"] == null),
                                           It.IsAny<List<Media>>(),
                                           It.IsAny<IRequestProcessor<Status>>()),
                                       Times.Once(),
                                       "TweetWithMedia shouldn't pass placeID parameter.");
        }

        [Fact]
        public void TweetWithMedia_DoesNot_Pass_Empty_placeID_via_Parameter_Dictionary_to_PostMedia()
        {
            this.placeID = "";

            StatusExtensions.TweetWithMedia(
                this.twitterCtx.Object, this.status, this.possiblySensitive,
                this.latitude, this.longitude, this.placeID, this.displayCoordinates,
                this.mediaItems, null);

            this.twitterExecute.Verify(exec =>
                                       exec.PostMedia(
                                           It.IsAny<string>(),
                                           It.Is<IDictionary<string, string>>(parms => parms["place_id"] == null),
                                           It.IsAny<List<Media>>(),
                                           It.IsAny<IRequestProcessor<Status>>()),
                                       Times.Once(),
                                       "TweetWithMedia shouldn't pass placeID parameter.");
        }

        [Fact]
        public void TweetWithMedia_Passes_displayCoordinates_via_Parameter_Dictionary_to_PostMedia()
        {
            StatusExtensions.TweetWithMedia(
                this.twitterCtx.Object, this.status, this.possiblySensitive,
                this.latitude, this.longitude, this.placeID, this.displayCoordinates,
                this.mediaItems, null);

            this.twitterExecute.Verify(exec =>
                                       exec.PostMedia(
                                           It.IsAny<string>(),
                                           It.Is<IDictionary<string, string>>(parms => parms["display_coordinates"] == displayCoordinates.ToString()),
                                           It.IsAny<List<Media>>(),
                                           It.IsAny<IRequestProcessor<Status>>()),
                                       Times.Once(),
                                       "TweetWithMedia didn't pass displayCoordinates parameter properly.");
        }

        [Fact]
        public void TweetWithMedia_DoesNot_Pass_false_displayCoordinates_via_Parameter_Dictionary_to_PostMedia()
        {
            this.displayCoordinates = false;

            StatusExtensions.TweetWithMedia(
                this.twitterCtx.Object, this.status, this.possiblySensitive,
                this.latitude, this.longitude, this.placeID, this.displayCoordinates,
                this.mediaItems, null);

            this.twitterExecute.Verify(exec =>
                                       exec.PostMedia(
                                           It.IsAny<string>(),
                                           It.Is<IDictionary<string, string>>(parms => parms["display_coordinates"] == null),
                                           It.IsAny<List<Media>>(),
                                           It.IsAny<IRequestProcessor<Status>>()),
                                       Times.Once(),
                                       "TweetWithMedia shouldn't pass displayCoordinates parameter.");
        }

        [Fact]
        public void TweetWithMedia_DoesNot_Pass_inReplyToStatusID_via_Parameter_Dictionary_to_PostMedia()
        {
            this.inReplyToStatusID = StatusExtensions.NoReply;

            StatusExtensions.TweetWithMedia(
                this.twitterCtx.Object, this.status, this.possiblySensitive,
                this.latitude, this.longitude, this.placeID, this.displayCoordinates,
                this.mediaItems, null);

            this.twitterExecute.Verify(exec =>
                                       exec.PostMedia(
                                           It.IsAny<string>(),
                                           It.Is<IDictionary<string, string>>(parms => parms["in_reply_to_status_id"] == null),
                                           It.IsAny<List<Media>>(),
                                           It.IsAny<IRequestProcessor<Status>>()),
                                       Times.Once(),
                                       "TweetWithMedia shouldn't pass inReplyToStatusID parameter.");
        }

        [Fact]
        public void TweetWithMedia_Passes_latitude_via_Parameter_Dictionary_to_PostMedia()
        {
            StatusExtensions.TweetWithMedia(
                this.twitterCtx.Object, this.status, this.possiblySensitive,
                this.latitude, this.longitude, this.placeID, this.displayCoordinates,
                this.mediaItems, null);

            this.twitterExecute.Verify(exec =>
                                       exec.PostMedia(
                                           It.IsAny<string>(),
                                           It.Is<IDictionary<string, string>>(parms => parms["lat"] == latitude.ToString()),
                                           It.IsAny<List<Media>>(),
                                           It.IsAny<IRequestProcessor<Status>>()),
                                       Times.Once(),
                                       "TweetWithMedia didn't pass latitude parameter properly.");
        }

        [Fact]
        public void TweetWithMedia_Passes_longitude_via_Parameter_Dictionary_to_PostMedia()
        {
            StatusExtensions.TweetWithMedia(
                this.twitterCtx.Object, this.status, this.possiblySensitive,
                this.latitude, this.longitude, this.placeID, this.displayCoordinates,
                this.mediaItems, null);

            this.twitterExecute.Verify(exec =>
                                       exec.PostMedia(
                                           It.IsAny<string>(),
                                           It.Is<IDictionary<string, string>>(parms => parms["long"] == longitude.ToString()),
                                           It.IsAny<List<Media>>(),
                                           It.IsAny<IRequestProcessor<Status>>()),
                                       Times.Once(),
                                       "TweetWithMedia didn't pass longitude parameter properly.");
        }

        [Fact]
        public void TweetWithMedia_DoesNot_Pass_NoCoordinate_Lat_And_Long_To_PostMedia()
        {
            this.latitude = StatusExtensions.NoCoordinate;
            this.longitude = StatusExtensions.NoCoordinate;

            StatusExtensions.TweetWithMedia(
                this.twitterCtx.Object, this.status, this.possiblySensitive,
                this.latitude, this.longitude, this.placeID, this.displayCoordinates,
                this.mediaItems, null);

            this.twitterExecute.Verify(exec =>
                                       exec.PostMedia(
                                           It.IsAny<string>(),
                                           It.Is<IDictionary<string, string>>(
                                               parms =>
                                               parms["lat"] == null &&
                                               parms["long"] == null),
                                           It.IsAny<List<Media>>(),
                                           It.IsAny<IRequestProcessor<Status>>()),
                                       Times.Once(),
                                       "Lat and long should be null.");
        }

        [Fact]
        public void TweetWithMedia_Translates_Localized_Lat_And_Long()
        {
            string latString = this.latitude.ToString();
            string longString = this.longitude.ToString();
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(this.frenchLocale);

            StatusExtensions.TweetWithMedia(
                this.twitterCtx.Object, this.status, this.possiblySensitive,
                this.latitude, this.longitude, this.placeID, this.displayCoordinates,
                this.mediaItems, null);

            this.twitterExecute.Verify(exec =>
                                       exec.PostMedia(
                                           It.IsAny<string>(),
                                           It.Is<IDictionary<string, string>>(
                                               parms =>
                                               parms["lat"] == latString &&
                                               parms["long"] == longString),
                                           It.IsAny<List<Media>>(),
                                           It.IsAny<IRequestProcessor<Status>>()),
                                       Times.Once(),
                                       "Invariant culture not applied to Lat and/or long.");
        }

        [Fact]
        public void TweetWithMedia_Passes_Media_to_PostMedia()
        {
            StatusExtensions.TweetWithMedia(
                this.twitterCtx.Object, this.status, this.possiblySensitive,
                this.latitude, this.longitude, this.placeID, this.displayCoordinates,
                this.mediaItems, null);

            this.twitterExecute.Verify(exec =>
                                       exec.PostMedia(
                                           It.IsAny<string>(),
                                           It.IsAny<IDictionary<string, string>>(),
                                           It.Is<List<Media>>(media => object.ReferenceEquals(media, this.mediaItems)),
                                           It.IsAny<IRequestProcessor<Status>>()),
                                       Times.Once(),
                                       "TweetWithMedia didn't pass mediaItems properly.");
        }

        [Fact]
        public void TweetWithMedia_Requires_NonNull_mediaItems()
        {
            this.mediaItems = null;

            ArgumentNullException ex =
            Assert.Throws<ArgumentNullException>(() =>
                StatusExtensions.TweetWithMedia(
                    this.twitterCtx.Object, this.status, this.possiblySensitive,
                    this.latitude, this.longitude, this.placeID, this.displayCoordinates,
                    this.mediaItems, null));

            Assert.Equal("mediaItems", ex.ParamName);
        }

        [Fact]
        public void TweetWithMedia_Requires_At_Least_One_Media()
        {
            this.mediaItems = new List<Media>();

            ArgumentException ex =
            Assert.Throws<ArgumentException>(() =>
                StatusExtensions.TweetWithMedia(
                    this.twitterCtx.Object, this.status, this.possiblySensitive,
                    this.latitude, this.longitude, this.placeID, this.displayCoordinates,
                    this.mediaItems, null));

            Assert.Equal("mediaItems", ex.ParamName);
        }

        [Fact]
        public void TweetWithMedia_Calls_TwitterContext_CreateRequestProcessor()
        {
            StatusExtensions.TweetWithMedia(
                this.twitterCtx.Object, this.status, this.possiblySensitive,
                this.latitude, this.longitude, this.placeID, this.displayCoordinates,
                this.mediaItems, null);

            this.twitterCtx.Verify(
                ctx => ctx.CreateRequestProcessor<Status>(),
                Times.Once(),
                "Didn't call CreateRequestProcessor.");
        }

        [Fact]
        public void TweetWithMedia_Passes_IRequestProcessor_To_PostMedia()
        {
            StatusExtensions.TweetWithMedia(
                this.twitterCtx.Object, this.status, this.possiblySensitive,
                this.latitude, this.longitude, this.placeID, this.displayCoordinates,
                this.mediaItems, null);

            this.twitterExecute.Verify(exec =>
                                       exec.PostMedia(
                                           It.IsAny<string>(),
                                           It.IsAny<IDictionary<string, string>>(),
                                           It.IsAny<List<Media>>(),
                                           It.Is<IRequestProcessor<Status>>(
                                               reqProc =>
                                               reqProc != null &&
                                               reqProc is IRequestProcessor<Status>)),
                                       Times.Once(),
                                       "TweetWithMedia didn't pass reqProc properly.");
        }

        [Fact]
        public void TweetWithMedia_Returns_Status()
        {
            Status tweet = StatusExtensions.TweetWithMedia(
                this.twitterCtx.Object, this.status, this.possiblySensitive,
                this.latitude, this.longitude, this.placeID, this.displayCoordinates,
                this.mediaItems, null);

            Assert.Equal("Test", tweet.Text);
        }

        [Fact]
        public void TweetWithMedia_Sets_Callback()
        {
            this.twitterExecute.SetupSet(exec => exec.AsyncCallback);

            StatusExtensions.TweetWithMedia(
                this.twitterCtx.Object, this.status, this.possiblySensitive,
                this.latitude, this.longitude, this.placeID, this.displayCoordinates,
                this.mediaItems, null);

            this.twitterExecute.VerifySet(exec => exec.AsyncCallback, Times.Once());
        }
    }
}
