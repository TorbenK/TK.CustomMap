# Extended Map Control for Xamarin.Forms.Maps
[![NuGet](https://badge.fury.io/nu/TK.CustomMap.svg)](https://www.nuget.org/packages/TK.CustomMap/) [![Build status](https://ci.appveyor.com/api/projects/status/g2pv4rckrudbsm2h?svg=true)](https://ci.appveyor.com/project/TorbenK/tk-custommap)

This is an extended, bindable map control based on Xamarin.Forms.Maps. Here is a list of some available features:

* Customize pins
 * Custom image
 * Draggable
 * and more
* Add overlays
 * Circles
 * Lines
 * Custom polygons
* Add and calculate routes
* Start place prediction queries
* Add custom map tiles via url template
* Additional commands available
 * Long press
 * Map click
 * and many more

Look at the [wiki](https://github.com/TorbenK/TK.CustomMap/wiki) for all available features and documentation.

# Setup

Install the [NuGet package](https://www.nuget.org/packages/TK.CustomMap/).

## Android

Add your Google Maps API key to your `AndroidManifest.xml`.

```XAML
<meta-data android:name="com.google.android.maps.v2.API_KEY" android:value="YOUR API KEY" />
```

Following permissions should be added to your `AndroidManifest.xml`

```XAML
<uses-permission android:name="android.permission.ACCESS_NETWORK_STATE" />
<uses-permission android:name="android.permission.ACCESS_FINE_LOCATION" />
<uses-permission android:name="android.permission.ACCESS_COARSE_LOCATION" />
```

If you plan to use the automatic route calculation, a valid key for the [Google Maps Directions API](https://developers.google.com/maps/documentation/directions/) is required. You can set it up anywhere in your PCL, simply call `GmsDirection.Init("YOUR API KEY");`.

If you plan to use the `NativePlacesApi` make sure you have a valid key for the [Google Places API](https://developers.google.com/places/) and set the key in your `AndroidManifest.xml`. Replace the following line

```XAML
<meta-data android:name="com.google.android.maps.v2.API_KEY" android:value="YOUR API KEY" />
``` 
with

```XAML
<meta-data android:name="com.google.android.geo.API_KEY" android:value="YOUR API KEY" />
```

## iOS

Make sure you call `TKCustomMapRenderer.InitMapRenderer();` in `FinishedLaunching` in your `AppDelegate.cs`.

If you plan to use the `NativePlacesApi` also call `NativePlacesApi.Init();`

```CSharp
public override bool FinishedLaunching(UIApplication app, NSDictionary options)
{
    Forms.Init();
    FormsMaps.Init();
    TKCustomMapRenderer.InitMapRenderer();
    NativePlacesApi.Init();
    
    LoadApplication(new App());
    return base.FinishedLaunching(app, options);
}
```

# Contributions

If you want to contribute, please use the Development branch to submit PRs.

# Known Issues

## Image offset

If you notice wrong offsets or moving images during zoom, you can use the `Anchor` property to properly adjust the anchor point.

# Video

## Android

[![Android](http://i.imgur.com/HDrntbk.png)](https://youtu.be/tmIxX3LVSic "Android")

## iOS

[![iOS](http://i.imgur.com/q8uuh7q.png)](https://youtu.be/yJoCVe7t7e4 "iOS")

## Contributors

* [TorbenK](https://github.com/TorbenK)
* [yumshinetech](https://github.com/yumshinetech)
* [MithrilMan](https://github.com/MithrilMan)
* [CliffCawley](https://github.com/CliffCawley)
* [JeanCollas](https://github.com/JeanCollas)
* [Falco20019](https://github.com/Falco20019)
* [RanaInside](https://github.com/RanaInside)
* [xalikoutis](https://github.com/xalikoutis)
* [jessejiang0214](https://github.com/jessejiang0214)
* [dapug](https://github.com/dapug)
* [hardcodet](https://github.com/hardcodet)
* [Spierki](https://github.com/Spierki)
* [DennisWelu](https://github.com/DennisWelu)
* [SavikPavel](https://github.com/SavikPavel)
* [krisrok](https://github.com/krisrok)


