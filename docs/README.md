


# Getting started

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
Make sure you call 
```CSharp 
TKGoogleMaps.Init(this, bundle); 
```    

<br />

## Shared

Include namespace in your XAML file:
```XML
<ContentPage xmlns:tk="clr-namespace:TK.CustomMap;assembly=TK.CustomMap" />
```

Setup map:
```XML
<tk:TKCustomMap MapRegion="{Binding MapRegion}"
                IsShowingUser="{Binding IsShowingUser}"
                IsClusteringEnabled="{Binding IsClusteringEnabled}"
                Pins="{Binding Pins}"
                PinSelectedCommand="{Binding PinSelectedCommand}"
                PinDragEndCommand="{Binding PinDragEndCommand}"
                Routes="{Binding Routes}"
                RouteClickedCommand="{Binding RouteClickedCommand}"
                Polylines="{Binding Polylines}"
                Polygons="{Binding Polygons}"
                Circles="{Binding Circles}"
                TilesUrlOptions="{Binding TileOptions}"
                MapFunctions="{Binding MapFunctions}"
                SelectedPin="{Binding SelectedPin}"
                MapClickedCommand="{Binding MapClickedCommand}"
                MapLongPressCommand="{Binding MapLongPressCommand}"
                CalloutClickedCommand="{Binding CalloutClickedCommand}" />
```


# Main features

|| iOS | Android |
|:------:|:------:|:------:|
| **Customize pins** | X | X |
| **Draggable pins** | X | X |
| **Url Tile overlays** | X | X |
| **Pin clustering** | X | X |
| **Place predictions API** | X | X |
| **Route calculation** | X | X |
| **Circles** | X | X |
| **Polylines** | X | X |
| **Polygons** | X | X |
