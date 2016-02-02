# Extended Map Control for Xamarin.Forms.Maps
[![NuGet](https://badge.fury.io/nu/TK.CustomMap.svg)](https://www.nuget.org/packages/TK.CustomMap/) [![Build status](https://ci.appveyor.com/api/projects/status/t3rlse4w5omu44sy?svg=true)](https://ci.appveyor.com/project/TorbenK/tk-custommap)

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

# Video

## Android

[![Android](http://i.imgur.com/HDrntbk.png)](https://youtu.be/tmIxX3LVSic "Android")

## iOS

[![iOS](http://i.imgur.com/q8uuh7q.png)](https://youtu.be/yJoCVe7t7e4 "iOS")

## Contributors

yumshinetech

