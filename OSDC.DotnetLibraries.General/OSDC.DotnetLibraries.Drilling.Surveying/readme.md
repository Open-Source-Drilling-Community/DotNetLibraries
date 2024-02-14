# Background
This package is developed as part of the Society of Petroleum (SPE) Open Source Drilling Community, a sub-committee of the Drilling System Automation Technical Section.
This package contains classes to perform survey calculations.

# Survey
A `Survey` is a subclass of `CurvilinearPoint3D`. For that reason, it is a `Point3D` with `X`, `Y` and `Z` components, but it is also defined
on a curve and has therefore a curvilinear `Abscissa` and a tangent defined by an `Inclination` and an `Azimuth`. In the drilling world a different
terminology than the mathematical one is used and synonyms properties are added as a matter of convenience. For instance `MD` is a synonym for `Abscissa`, 
and `TVD` is a synonym for `Z`. Meta data are also provided, meaning that some properties are redefined locally to allow for adding `Attribute` like
`DrillingPhysicalQuantity` or `PositionReference` for instance. 

Usually, a `Survey` belongs to a trajectory and therefore it is interesting maintain information about the local curvature at that point. For that reason, 
there are four additional properties `Curvature`, `Toolface`, `BUR` and `TUR`, where the last two stands for build-up rate and turn rate, respectively.

The class `Survey` defines two additional properties: `Latitude` and `Longitude`. This is 
because the drilling data model makes only use of globally defined values, meaning that a well position, i.e., a `Survey` must be defined uniquely
on the Earth. This is achieved by using `Latidude` and `Longitude` on the WGS84 spheroid, which is the reference Earth spheroid for all geodetic
conversions. Then remains the problem of what `X`, `Y` and `Z` mean when considering that they shall be defined globally for any position on the Earth.
It is desirable to keep the meaning of `Z` to be a depth in the vertical direction. Then `X` and `Y` should be somewhat related to `Latitude` and `Longitude`
but with a physical quantity of dimension Length. The solution adopted is that `X` is the length of the arc following the meridian at that 
`Longitude` and originating from the equator and counted positively in the North direction. Similarly, `Y` is the length of the arc following the parallel 
at that `Latitude` and originating from the Greenwich meridian and counted positively in the East direction. In other words, `X` and `Y` are coordinates
in a Riemannian manifold defined the Earth spheroid. Therefore two synonym properties are introduced: `RiemannianNorth`which corresponds to `X`
and RiemannianEast which corresponds to `Y`. 

As a reminder, a manifold is a topological space that locally resembles an Euclidian space near each point. A smooth
manifold is a differentiable manifold that is locally similar to a vector space to allow the application of calculus. A Riemannian manifold is a smooth manifold
equipped with a positive-definite inner product on the tangent space at each point of the manifold, and therefore that allows to work with metrics.

The next section explains how `Latitude` and `Longitude` are converted to `X` and `Y`.

## Conversion from Latitude-Longitude to Riemannian Coordinates

The earth is modelled as an oblate, i.e., a spheroid flatened at the pole. At a given latitude, a path on the Earth is a circle. Let us consider that the origin of 
longitudes is Greenwich and that the Earth is modelled by a semi-long axis, $$a$$, and a flatening, $$f$$. The flatening is defined as:
$$f = \frac{{a - b}}{{a}}$$
where $$b$$ is the semi-short axis. Therefore the semi short axis can be expressed as:
$$b = a - f \cdot a$$

The radius of the Earth at a given latitude, $$\phi$$ is given by:
$$R(\phi) = \frac{{a \cdot \sqrt{{\cos^2(\phi) + \frac{{b^2}}{{a^2}} \cdot \sin^2(\phi)}}}}{{\sqrt{1 - f \cdot (2 - f) \cdot \sin^2(\phi)}}}$$

<svg width="700" height="300" xmlns="http://www.w3.org/2000/svg" xmlns:xlink="http://www.w3.org/1999/xlink" overflow="hidden">
<g>
<rect x="0" y="0" width="700" height="300" fill="#FFFFFF"/>
<path d="M136.5 141C136.5 76.6589 226.491 24.5001 337.5 24.5001 448.509 24.5001 538.5 76.6589 538.5 141 538.5 205.341 448.509 257.5 337.5 257.5 226.491 257.5 136.5 205.341 136.5 141Z" stroke="#000000" stroke-width="1.33333" stroke-miterlimit="8" fill="none" fill-rule="evenodd"/>
<path d="M282.5 141C282.5 76.6589 306.005 24.5001 335 24.5001 363.995 24.5001 387.5 76.6589 387.5 141 387.5 205.341 363.995 257.5 335 257.5 306.005 257.5 282.5 205.341 282.5 141Z" stroke="#000000" stroke-width="1.33333" stroke-miterlimit="8" fill="none" fill-rule="evenodd"/>
<path d="M136.5 141C136.5 119.185 226.491 101.5 337.5 101.5 448.509 101.5 538.5 119.185 538.5 141 538.5 162.815 448.509 180.5 337.5 180.5 226.491 180.5 136.5 162.815 136.5 141Z" stroke="#000000" stroke-width="1.33333" stroke-miterlimit="8" fill="none" fill-rule="evenodd"/>
<text font-family="Calibri,Calibri_MSFontService,sans-serif" font-weight="400" font-size="24" transform="translate(9.6 94)">Equator</text>
<path d="M48.6383 106.197 140.944 148.27 140.668 148.876 48.3618 106.803ZM141.252 144.38 146.872 151.338 137.934 151.66Z"/>
<text font-family="Calibri,Calibri_MSFontService,sans-serif" font-weight="400" font-size="24" transform="translate(128.54 275)">Greenwich</text>
<path d="M0.214529-0.255125 48.2213 40.1127 47.7923 40.6229-0.214529 0.255125ZM49.5606 36.4482 53.1093 44.6584 44.412 42.5712Z" transform="matrix(1 0 0 -1 245.5 268.158)"/>
<path d="M255.5 141C255.5 76.6589 291.317 24.5001 335.5 24.5001 379.683 24.5001 415.5 76.6589 415.5 141 415.5 205.341 379.683 257.5 335.5 257.5 291.317 257.5 255.5 205.341 255.5 141Z" stroke="#000000" stroke-width="1.33333" stroke-miterlimit="8" stroke-dasharray="5.33333 4" fill="none" fill-rule="evenodd"/>
<path d="M173.5 78.5001C173.5 60.8269 247.597 46.5001 339 46.5001 430.403 46.5001 504.5 60.8269 504.5 78.5001 504.5 96.1732 430.403 110.5 339 110.5 247.597 110.5 173.5 96.1732 173.5 78.5001Z" stroke="#000000" stroke-width="1.33333" stroke-miterlimit="8" stroke-dasharray="5.33333 4" fill="none" fill-rule="evenodd"/>
<text font-family="Calibri,Calibri_MSFontService,sans-serif" font-weight="400" font-size="24" transform="translate(347.344 198)">0,0</text>
<path d="M411.682 107.294C416.256 129.694 416.098 153.579 411.229 175.847" stroke="#0070C0" stroke-width="4" stroke-miterlimit="8" fill="none" fill-rule="evenodd"/>
<path d="M414.179 106.458C405.4 107.331 396.234 108.05 386.791 108.607" stroke="#0070C0" stroke-width="4" stroke-miterlimit="8" fill="none" fill-rule="evenodd"/>
<text font-family="Calibri,Calibri_MSFontService,sans-serif" font-weight="400" font-size="24" transform="translate(431.699 148)">x</text>
<text font-family="Calibri,Calibri_MSFontService,sans-serif" font-weight="400" font-size="24" transform="translate(392.211 95)">y</text>
<path d="M335 3 335 279.232" stroke="#000000" stroke-width="2" stroke-miterlimit="8" stroke-dasharray="8 6 2 6" fill="none" fill-rule="evenodd"/>
<path d="M0.278086-0.183792 18.4004 27.2361 17.8442 27.6037-0.278086 0.183792ZM20.7241 24.1021 21.7981 32.9816 14.0501 28.5131Z" transform="matrix(-1 0 0 1 335.298 78.5001)"/>
<text font-family="Calibri,Calibri_MSFontService,sans-serif" font-weight="400" font-size="24" transform="translate(309.658 88)">R</text>
<path d="M557.807 140.46 116 140" stroke="#000000" stroke-width="2" stroke-miterlimit="8" stroke-dasharray="8 6 2 6" fill="none" fill-rule="evenodd"/>
<path d="M334.968 142.514 293.465 116.948 295.563 113.542 337.066 139.108ZM293.07 121.402 286 110 299.364 111.185Z" fill="#0070C0"/>
<path d="M0 0 47.6964 39.883" stroke="#000000" stroke-width="0.666667" stroke-miterlimit="8" fill="none" fill-rule="evenodd" transform="matrix(-1 0 0 1 333.196 140.5)"/>
<path d="M301.663 165.273C300.875 150.727 300.789 135.71 301.408 121.033" stroke="#000000" stroke-width="2" stroke-miterlimit="8" fill="none" fill-rule="evenodd"/>
<text font-family="Calibri,Calibri_MSFontService,sans-serif" font-weight="400" font-size="24" transform="translate(285.473 136)">φ</text>
<path d="M385.683 164.3C357.757 165.115 328.66 165.22 300.359 164.606" stroke="#000000" stroke-width="2" stroke-miterlimit="8" fill="none" fill-rule="evenodd"/>
<path d="M335.865 138.197 402.763 170.309 401.032 173.915 334.135 141.803ZM402.691 165.837 410.913 176.439 397.498 176.655Z" fill="#0070C0"/>
<text font-family="Calibri,Calibri_MSFontService,sans-serif" font-weight="400" font-size="24" transform="translate(341.513 160)">λ</text>
<path d="M382.5 178.5C382.5 177.395 383.395 176.5 384.5 176.5 385.605 176.5 386.5 177.395 386.5 178.5 386.5 179.605 385.605 180.5 384.5 180.5 383.395 180.5 382.5 179.605 382.5 178.5Z" stroke="#000000" stroke-width="1.33333" stroke-miterlimit="8" fill-rule="evenodd"/>
</g>
</svg>

At that latitude the $$y$$-coordinate (east-west) is the length of the circular arc counted from the Greenwich meridian, i.e., the longitude angle, $$\lambda$$:
$$y = R(\phi) \cdot \lambda$$
The $$x$$-coordinate (south-north) is the length of the elliptical arc counted from the equator using the latitude. 
This involves the elliptic integral of the second kind, denoted $$E(\phi, m)$ where $m=1- \frac{{b^2}}{{a^2}}$$. 
Its definition is: $$E(\phi, m) = \int_0^\phi \sqrt{1 - m \cdot \sin^2(t)} \, dt$$.
The definition of $$x$$ is then: $$x = a \cdot E(\phi, m)$$.

Conversely, to retrieve the latitude and longitude from the $$x$$ and $$y$$ coordinates, i.e., arc lengths, the following method is used:
$$\phi = E^{-1}(\frac{x}{a}, m)$$ and $$\lambda = \frac{y}{R(\phi)}$$.

The elliptic integral of the second kind is calculated using the special function defined in `OSDC.DotnetLibraries.General.Math`, 
namely `SpecialFunctions.EllipticE(phi, m)` and its inverse is `Elliptic.InverseEllipticE(x, m)`.

For short lateral displacements, considering that `X` and `Y` are cartesian coordinates does not introduce much error. This is actually what is assumed
by most Earth Model software used in E&P applications. So `X` and `Y` can be considered as respectively a sort of Northing and Easting coordinate.

## Conversion to Spherical Coordinate
At a given latitude, the radial distance to the centre of the Earth is given by: $$r = \frac{a}{\sqrt{1-e^2 \sin(\phi)}}$$ and where $$e=\sqrt{\frac{a^2-b^2}{a^2}}$$ is the eccentricity 
of the ellipse. 

The radial position of the point is $$r_p = r(\phi)-Z$$ where $$Z$$ is the vertical depth at that point.

Knowing the radial position, the latitude and the longitude, it is possible to calculate the cartesian coordinates of the point in a global coordinate
system centered at the Earth center and with X, Y and Z directions that are fixed orthogonal axes attached to the Earth. The method `GetSphericalPoint` returns a `SphericalPoint3D`
in this cartesian reference system. The X-direction passes by the equator and the Greenwich meridian. The Y-direction passes by the equator and is 90deg east
for the Greenwich meridian. The Z-direction passed by the North pole.
<svg width="700" height="330" xmlns="http://www.w3.org/2000/svg" xmlns:xlink="http://www.w3.org/1999/xlink" overflow="hidden">
<g>
<rect x="0" y="0" width="700" height="330" fill="#FFFFFF"/>
<path d="M136.5 169.5C136.5 104.883 226.491 52.5001 337.5 52.5001 448.509 52.5001 538.5 104.883 538.5 169.5 538.5 234.117 448.509 286.5 337.5 286.5 226.491 286.5 136.5 234.117 136.5 169.5Z" stroke="#000000" stroke-width="1.33333" stroke-miterlimit="8" fill="none" fill-rule="evenodd"/>
<path d="M282.5 169.5C282.5 104.883 306.005 52.5001 335 52.5001 363.995 52.5001 387.5 104.883 387.5 169.5 387.5 234.117 363.995 286.5 335 286.5 306.005 286.5 282.5 234.117 282.5 169.5Z" stroke="#000000" stroke-width="1.33333" stroke-miterlimit="8" fill="none" fill-rule="evenodd"/>
<path d="M136.5 169.5C136.5 147.961 226.491 130.5 337.5 130.5 448.509 130.5 538.5 147.961 538.5 169.5 538.5 191.039 448.509 208.5 337.5 208.5 226.491 208.5 136.5 191.039 136.5 169.5Z" stroke="#000000" stroke-width="1.33333" stroke-miterlimit="8" fill="none" fill-rule="evenodd"/>
<text font-family="Calibri,Calibri_MSFontService,sans-serif" font-weight="400" font-size="24" transform="translate(9.6 123)">Equator</text>
<path d="M48.6383 135.197 140.944 177.27 140.668 177.876 48.3618 135.803ZM141.252 173.38 146.872 180.338 137.934 180.66Z"/>
<text font-family="Calibri,Calibri_MSFontService,sans-serif" font-weight="400" font-size="24" transform="translate(128.54 304)">Greenwich</text>
<path d="M0.214529-0.255125 48.2213 40.1127 47.7923 40.6229-0.214529 0.255125ZM49.5606 36.4482 53.1093 44.6584 44.412 42.5712Z" transform="matrix(1 0 0 -1 245.5 297.158)"/>
<path d="M255.5 169.5C255.5 104.883 291.317 52.5001 335.5 52.5001 379.683 52.5001 415.5 104.883 415.5 169.5 415.5 234.117 379.683 286.5 335.5 286.5 291.317 286.5 255.5 234.117 255.5 169.5Z" stroke="#000000" stroke-width="1.33333" stroke-miterlimit="8" stroke-dasharray="5.33333 4" fill="none" fill-rule="evenodd"/>
<path d="M173.5 107.5C173.5 89.8269 247.597 75.5001 339 75.5001 430.403 75.5001 504.5 89.8269 504.5 107.5 504.5 125.173 430.403 139.5 339 139.5 247.597 139.5 173.5 125.173 173.5 107.5Z" stroke="#000000" stroke-width="1.33333" stroke-miterlimit="8" stroke-dasharray="5.33333 4" fill="none" fill-rule="evenodd"/>
<text font-family="Calibri,Calibri_MSFontService,sans-serif" font-weight="400" font-size="24" transform="translate(347.344 227)">0,0</text>
<text font-family="Calibri,Calibri_MSFontService,sans-serif" font-weight="400" font-size="24" transform="translate(480.782 285)">X</text>
<text font-family="Calibri,Calibri_MSFontService,sans-serif" font-weight="400" font-size="24" transform="translate(561.776 137)">Y</text>
<path d="M336.049 38.6593 336.108 46.6591 334.108 46.6738 334.049 38.674ZM336.152 52.6589 336.167 54.6589 334.167 54.6736 334.152 52.6736ZM336.211 60.6587 336.27 68.6585 334.27 68.6732 334.211 60.6734ZM336.314 74.6583 336.329 76.6583 334.329 76.673 334.314 74.673ZM336.373 82.6581 336.432 90.6579 334.432 90.6726 334.373 82.6728ZM336.476 96.6577 336.49 98.6577 334.49 98.6724 334.476 96.6724ZM336.535 104.658 336.593 112.657 334.593 112.672 334.535 104.672ZM336.638 118.657 336.652 120.657 334.652 120.672 334.638 118.672ZM336.696 126.657 336.755 134.657 334.755 134.671 334.696 126.672ZM336.799 140.657 336.814 142.656 334.814 142.671 334.799 140.671ZM336.858 148.656 336.917 156.656 334.917 156.671 334.858 148.671ZM336.961 162.656 336.976 164.656 334.976 164.671 334.961 162.671ZM331.059 40.0292 335 32 339.059 39.9704Z"/><path d="M0.130962-0.306529 72.2806 30.5188 72.0187 31.1319-0.130962 0.306529ZM72.4952 26.6232 78.2803 33.4446 69.3521 33.9799Z" transform="matrix(1 0 0 -1 335.5 168.945)"/><path d="M557.807 169.46 116 169" stroke="#000000" stroke-width="2" stroke-miterlimit="8" stroke-dasharray="8 6 2 6" fill="none" fill-rule="evenodd"/>
<path d="M334.968 171.514 293.465 145.948 295.563 142.542 337.066 168.108ZM293.07 150.402 286 139 299.364 140.185Z" fill="#0070C0"/><path d="M0 0 47.6964 39.883" stroke="#000000" stroke-width="0.666667" stroke-miterlimit="8" fill="none" fill-rule="evenodd" transform="matrix(-1 0 0 1 333.196 169.5)"/>
<path d="M301.658 193.778C300.876 179.228 300.79 164.211 301.404 149.531" stroke="#000000" stroke-width="2" stroke-miterlimit="8" fill="none" fill-rule="evenodd"/>
<text font-family="Calibri,Calibri_MSFontService,sans-serif" font-weight="400" font-size="24" transform="translate(285.473 165)">φ</text>
<path d="M385.683 192.3C357.757 193.115 328.66 193.22 300.359 192.606" stroke="#000000" stroke-width="2" stroke-miterlimit="8" fill="none" fill-rule="evenodd"/>
<path d="M335.865 167.197 402.763 199.309 401.032 202.915 334.135 170.803ZM402.691 194.837 410.913 205.439 397.498 205.655Z" fill="#0070C0"/>
<text font-family="Calibri,Calibri_MSFontService,sans-serif" font-weight="400" font-size="24" transform="translate(341.513 189)">λ</text>
<path d="M382.5 207C382.5 205.619 383.395 204.5 384.5 204.5 385.605 204.5 386.5 205.619 386.5 207 386.5 208.381 385.605 209.5 384.5 209.5 383.395 209.5 382.5 208.381 382.5 207Z" stroke="#000000" stroke-width="1.33333" stroke-miterlimit="8" fill-rule="evenodd"/>
<text font-family="Calibri,Calibri_MSFontService,sans-serif" font-weight="400" font-size="24" transform="translate(338.39 142)">R(</text>
<text font-family="Calibri,Calibri_MSFontService,sans-serif" font-weight="400" font-size="24" transform="translate(358.723 142)">φ</text>
<text font-family="Calibri,Calibri_MSFontService,sans-serif" font-weight="400" font-size="24" transform="translate(374.39 142)">)</text>
<path d="M335.617 167.213 341.914 172.147 340.68 173.722 334.383 168.787ZM346.636 175.848 348.211 177.082 346.977 178.656 345.403 177.422ZM352.933 180.783 359.23 185.717 357.997 187.291 351.7 182.357ZM363.953 189.418 365.527 190.651 364.294 192.226 362.719 190.992ZM370.25 194.352 376.547 199.287 375.313 200.861 369.016 195.926ZM381.27 202.987 382.844 204.221 381.61 205.795 380.036 204.562ZM387.567 207.922 393.864 212.856 392.63 214.43 386.333 209.496ZM398.586 216.557 400.16 217.791 398.927 219.365 397.353 218.131ZM404.883 221.491 411.18 226.426 409.947 228 403.65 223.066ZM415.903 230.127 417.477 231.36 416.243 232.935 414.669 231.701ZM422.2 235.061 428.497 239.996 427.263 241.57 420.966 236.635ZM433.219 243.696 434.794 244.93 433.56 246.504 431.986 245.271ZM439.516 248.631 445.813 253.565 444.58 255.139 438.283 250.205ZM450.536 257.266 452.11 258.5 450.877 260.074 449.302 258.84ZM456.833 262.2 463.13 267.135 461.896 268.709 455.599 263.775ZM467.853 270.836 469.427 272.069 468.193 273.643 466.619 272.41ZM471.349 269.764 475.179 277.847 466.415 276.061Z"/><path d="M0.154663-0.987967 8.0584 0.249336 7.74907 2.22527-0.154663 0.987967ZM13.9862 1.17731 15.9621 1.48664 15.6528 3.46257 13.6769 3.15325ZM21.8899 2.41462 29.7937 3.65192 29.4844 5.62785 21.5806 4.39055ZM35.7215 4.5799 37.6974 4.88922 37.3881 6.86516 35.4122 6.55583ZM43.6252 5.8172 51.529 7.0545 51.2196 9.03044 43.3159 7.79313ZM57.4568 7.98248 59.4327 8.29181 59.1234 10.2677 57.1474 9.95841ZM65.3605 9.21978 73.2642 10.4571 72.9549 12.433 65.0512 11.1957ZM79.192 11.3851 81.168 11.6944 80.8586 13.6703 78.8827 13.361ZM87.0958 12.6224 94.9995 13.8597 94.6902 15.8356 86.7864 14.5983ZM100.927 14.7876 102.903 15.097 102.594 17.0729 100.618 16.7636ZM108.831 16.0249 116.735 17.2623 116.425 19.2382 108.522 18.0009ZM122.663 18.1902 124.639 18.4996 124.329 20.4755 122.353 20.1662ZM130.566 19.4275 138.47 20.6648 138.161 22.6408 130.257 21.4035ZM144.398 21.5928 146.374 21.9021 146.064 23.8781 144.089 23.5687ZM152.302 22.8301 160.205 24.0674 159.896 26.0434 151.992 24.8061ZM166.133 24.9954 168.109 25.3047 167.8 27.2807 165.824 26.9713ZM174.037 26.2327 181.941 27.47 181.631 29.4459 173.728 28.2086ZM187.868 28.398 189.844 28.7073 189.535 30.6832 187.559 30.3739ZM195.772 29.6353 203.676 30.8726 203.367 32.8485 195.463 31.6112ZM209.604 31.8006 211.58 32.1099 211.27 34.0858 209.294 33.7765ZM215.095 29.6237 222.381 34.8129 213.858 37.5275Z" transform="matrix(1 0 0 -1 336 169.813)"/>
<text font-family="Calibri,Calibri_MSFontService,sans-serif" font-weight="400" font-size="24" transform="translate(329.926 24)">Z</text>
<path d="M411 136 411 196.879" stroke="#4472C4" stroke-width="2" stroke-miterlimit="8" stroke-dasharray="8 6" fill="none" fill-rule="evenodd"/>
<path d="M0 0 37.9565 0.000104987" stroke="#4472C4" stroke-width="2" stroke-miterlimit="8" stroke-dasharray="8 6" fill="none" fill-rule="evenodd" transform="matrix(-1 0 0 1 410.957 197)"/>
<path d="M410.313 197.018 366 165" stroke="#4472C4" stroke-width="2" stroke-miterlimit="8" stroke-dasharray="8 6" fill="none" fill-rule="evenodd"/><path d="M412.237 136.412 336 99" stroke="#4472C4" stroke-width="2" stroke-miterlimit="8" stroke-dasharray="8 6" fill="none" fill-rule="evenodd"/>
<text fill="#0070C0" font-family="Calibri,Calibri_MSFontService,sans-serif" font-weight="400" font-size="24" transform="translate(356.788 205)">x</text>
<text fill="#0070C0" font-family="Calibri,Calibri_MSFontService,sans-serif" font-weight="400" font-size="24" transform="translate(358.027 162)">y</text>
<text fill="#0070C0" font-family="Calibri,Calibri_MSFontService,sans-serif" font-weight="400" font-size="24" transform="translate(321.943 102)">z</text>
</g>
</svg>

## Example of Riemannian and Cartesian coordinate transformation from Latitude and Longitude

Here is an example of a short program that takes `Survey` described in `Latitude`, `Longitude` and `TVD` and that displays the conversion
in the Riemannian space and in the Cartesian space.

<pre>
```csharp
using OSDC.DotnetLibraries.Drilling.Surveying;
using OSDC.DotnetLibraries.General.Math;
using System.Globalization;

namespace DrillingProperties
{
    class Example
    {
        static void Main()
        {
            // a underground position at Norce, Stavanger, Norway
            Survey survey = new Survey() { TVD = 500, Latitude = 58.93438 * System.Math.PI / 180.0, Longitude = 5.70725 * System.Math.PI / 180.0 };
            Console.WriteLine("RiemannianNorth: " + survey.RiemannianNorth?.ToString("F3", CultureInfo.InvariantCulture) + " m, RiemannianEast: " + survey.RiemannianEast?.ToString("F3", CultureInfo.InvariantCulture) + " m");

            SphericalPoint3D? sphericalPoint3D = survey.GetSphericalPoint();
            if (sphericalPoint3D != null)
            {
                Console.WriteLine("CartesianX: " + sphericalPoint3D.X?.ToString("F3", CultureInfo.InvariantCulture) + " m, CartesianY: " + sphericalPoint3D.Y?.ToString("F3", CultureInfo.InvariantCulture) + " m, CartesianZ: " + sphericalPoint3D.Z?.ToString("F3", CultureInfo.InvariantCulture) + " m");
            }
        }
    }
}
```
</pre>

The execution result is:

<pre>
RiemannianNorth: 6560503.255 m, RiemannianEast: 635328.164 m
CartesianX: 3284101.435 m, CartesianY: 328216.605 m, CartesianZ: 5478668.203 m
</pre>

## Minimum Curvature Method and Interpolation between two `Survey`
The minimum curvature method can be used to calculate the spatial position of a point from a previous and a measurement of `MD`, `Inclination` and `Azimuth`.
The method is called `CompleteSIA`. The calculation uses equations (9), (10) and (11) of the paper by Sawaryn and Thorogood (2005) ([https://doi.org/10.2118/84246-PA](https://doi.org/10.2118/84246-PA))

When the argument is a `Survey`, it also calculates `Curvature`, `Toolface`, `BUR` and `TUR`. The `Toolface` is calculated using eq. (48) from the paper by 
Sawaryn and Thorogood (2005) ([https://doi.org/10.2118/84246-PA](https://doi.org/10.2118/84246-PA)). Since the toolface, the build-up rate and the turn-rate are not constant along a circular arc, a 
`CurvilinearPoint3D` is interpolated 0.1 m before the final point on the circular arc and the toolface is calculated between that interpolated point and the final point. Similarly,
the build-up rate and the turn rate are usually not constant along a circular arc. Therefore they are estimated between the interpolated point and the final point 
of the circular arc.

There is also a method to interpolate either a `CurvilinearPoint3D` or a `Survey` in between a `Survey` and `ICurvilinear3D` using the minimum curvature method. This is method
is called `InterpolateAtAbscissa`. If the requested result is a `Survey`, then the `Curvature`, `Toolface`, `BUR` and `TUR are also calculated locally.

Here is an example illustrating how these methods can be used.

<pre>
```csharp
using OSDC.DotnetLibraries.Drilling.Surveying;
using OSDC.DotnetLibraries.General.Common;
using System.Globalization;

namespace DrillingProperties
{
    class Example
    {
        static void Main()
        {
            // an underground position at Norce, Stavanger, Norway
            Survey survey1 = new Survey() { TVD = 100, Latitude = 58.93438 * System.Math.PI / 180.0, Longitude = 5.70725 * System.Math.PI / 180.0, MD = 100.0, Inclination = 0, Azimuth = 0 };
            Survey survey2 = new Survey() { MD = 130.0, Inclination = 2.0 * Numeric.PI / 180.0, Azimuth = 30.0 * Numeric.PI / 180.0 };
            if (survey1.CompleteSIA(survey2))
            {
                Console.WriteLine("Calculated displacements: dZ= " + (survey2.TVD - survey1.TVD)?.ToString("F3", CultureInfo.InvariantCulture) +
                    " m, dNorth= " + (survey2.RiemannianNorth - survey1.RiemannianNorth)?.ToString("F3", CultureInfo.InvariantCulture) +
                    " m, dEast= " + (survey2.RiemannianEast - survey1.RiemannianEast)?.ToString("F3", CultureInfo.InvariantCulture) + " m");
                Survey survey3 = new Survey();
                survey1.InterpolateAtAbscissa(survey2, 110.0, survey3);
                Console.WriteLine("Interpolated survey: dZ= " + (survey3.TVD - survey1.TVD)?.ToString("F3", CultureInfo.InvariantCulture) +
                    " m, dNorth= " + (survey3.RiemannianNorth - survey1.RiemannianNorth)?.ToString("F3", CultureInfo.InvariantCulture) +
                    " m, dEast= " + (survey3.RiemannianEast - survey1.RiemannianEast)?.ToString("F3", CultureInfo.InvariantCulture) +
                    " m, Inclination= " + (survey3.Inclination * 180.0 / Numeric.PI)?.ToString("F3", CultureInfo.InvariantCulture) +
                    " °, Azimuth= " + (survey3.Azimuth * 180.0 / Numeric.PI)?.ToString("F3", CultureInfo.InvariantCulture) +
                    " °, Curvature= " + (survey3.Curvature * 180.0 * 30.0 / Numeric.PI)?.ToString("F3", CultureInfo.InvariantCulture) +
                    " °/30m, Toolface= " + (survey3.Toolface * 180.0 / Numeric.PI)?.ToString("F3", CultureInfo.InvariantCulture) +
                    " °, BUR= " + (survey3.BUR * 180.0 * 30.0 / Numeric.PI)?.ToString("F3", CultureInfo.InvariantCulture) +
                    " °/30m, TUR= " + (survey3.TUR * 180.0 * 30.0 / Numeric.PI)?.ToString("F3", CultureInfo.InvariantCulture) +
                    " °/30m");
            }
        }
    }
}
```
</pre>

And the execution result is:
<pre>
Calculated displacements: dZ= 29.994 m, dNorth= 0.453 m, dEast= 0.262 m
Interpolated survey: dZ= 10.000 m, dNorth= 0.050 m, dEast= 0.029 m, Inclination= 0.667 °, Azimuth= 30.000 °, Curvature= 2.000 °/30m, Toolface= 30.000 °, BUR= 2.000 °/30m, TUR= 0.000 °/30m
</pre>

# A Plain Trajectory: `SurveyList`
The class `SurveyList` represents a plain trajectory, i.e., a list of `Survey`. It is sub-class of `List<Survey>`. There is a `Calculate` method to 
calculate the `Survey` incrementally from the first one. The first survey of the list must be complete, i.e., with a defined `MD`, `Inclination`, `Azimuth`,
`TVD`, `RiemannianNorth`, `RiemannianEast`. 

There is also an `InterpolateAtAbscissa` method to obtain an interpolated `Survey` at any given `Abscissa`. The `Abscissa` must be between the first and 
last `Survey` of the `SurveyList`. The interpolation is based the minimum curvature method.

It is also possible to obtain a method to obtain a reinteropolated `SurveyList` based on an interpolation step and possibly a list of required abscissas.

Here is an example showing how to define a `SurveyList`, calculate it and obtain an interpolated `SurveyList`.

<pre>
```csharp
using OSDC.DotnetLibraries.Drilling.Surveying;
using OSDC.DotnetLibraries.General.Common;
using System.Globalization;

namespace DrillingProperties
{
    class Example
    {
        static void Main()
        {
            // an underground position at Norce, Stavanger, Norway
            double groundLevel = 39.7;
            Survey survey1 = new Survey() { TVD = -groundLevel, Latitude = 58.93414 * System.Math.PI / 180.0, Longitude = 5.7085 * System.Math.PI / 180.0, MD = -groundLevel, Inclination = 0, Azimuth = 0 };
            SurveyList traj = new SurveyList() { survey1,
                new Survey() { MD = 50.0 - groundLevel, Inclination = 0.9 * Numeric.PI / 180.0, Azimuth = -29.7 * Numeric.PI / 180.0 },
                new Survey() { MD = 100.0 - groundLevel, Inclination = 0.4 * Numeric.PI / 180.0, Azimuth = -95.1 * Numeric.PI / 180.0 },
                new Survey() { MD = 150.0 - groundLevel, Inclination = 0.7 * Numeric.PI / 180.0, Azimuth = 142.5 * Numeric.PI / 180.0 },
                new Survey() { MD = 200.0 - groundLevel, Inclination = 0.9 * Numeric.PI / 180.0, Azimuth = 58.2 * Numeric.PI / 180.0 },
                new Survey() { MD = 250.0 - groundLevel, Inclination = 0.6 * Numeric.PI / 180.0, Azimuth = 173.2 * Numeric.PI / 180.0 },
                new Survey() { MD = 300.0 - groundLevel, Inclination = 1.3 * Numeric.PI / 180.0, Azimuth = 143.6 * Numeric.PI / 180.0 },
                new Survey() { MD = 350.0 - groundLevel, Inclination = 6.6 * Numeric.PI / 180.0, Azimuth = 155.6 * Numeric.PI / 180.0 },
                new Survey() { MD = 400.0 - groundLevel, Inclination = 11.2 * Numeric.PI / 180.0, Azimuth = 143.7 * Numeric.PI / 180.0 },
            };
            if (traj.Calculate())
            {
                Console.WriteLine("Calculated Trajectory");
                PrintTrajectory(traj);
                SurveyList interpolatedTraj = traj.Interpolate(10.0, new List<double>() { 229.0- groundLevel });
                if (interpolatedTraj != null)
                {
                    Console.WriteLine("Interpolated Trajectory");
                    PrintTrajectory(interpolatedTraj);
                }
            }
        }
        static void PrintTrajectory(SurveyList? traj)
        {
            if (traj != null)
            {
                Console.WriteLine("MD (m)\tIncl (°)\tAz (°)\tTVD (m)\tRiem. North (m)\tRiem. East (m)\tDLS (°/30m)\tToolface (°)\tBUR (°/30m)\tTUR (°/30m)");
                foreach (var sv in traj)
                {
                    if (sv != null)
                    {
                        if (sv.Curvature != null && sv.Toolface != null && sv.BUR != null && sv.TUR != null)
                        {
                            Console.WriteLine(sv.MD.Value.ToString("F3", CultureInfo.InvariantCulture) + "\t" +
                                (sv.Inclination.Value * 180.0 / Numeric.PI).ToString("F3", CultureInfo.InvariantCulture) + "\t" +
                                (sv.Azimuth.Value * 180.0 / Numeric.PI).ToString("F3", CultureInfo.InvariantCulture) + "\t\t" +
                                sv.TVD.Value.ToString("F3", CultureInfo.InvariantCulture) + "\t" +
                                sv.RiemannianNorth.Value.ToString("F3", CultureInfo.InvariantCulture) + "\t" +
                                sv.RiemannianEast.Value.ToString("F3", CultureInfo.InvariantCulture) + "\t" +
                                (sv.Curvature.Value * 30.0 * 180.0 / Numeric.PI).ToString("F3", CultureInfo.InvariantCulture) + "\t" +
                                (sv.Toolface.Value * 180.0 / Numeric.PI).ToString("F3", CultureInfo.InvariantCulture) + "\t" +
                                (sv.BUR.Value * 30.0 * 180.0 / Numeric.PI).ToString("F3", CultureInfo.InvariantCulture) + "\t" +
                                (sv.TUR.Value * 30.0 * 180.0 / Numeric.PI).ToString("F3", CultureInfo.InvariantCulture));
                        }
                        else
                        {
                            Console.WriteLine(sv.MD.Value.ToString("F3", CultureInfo.InvariantCulture) + "\t" +
                                (sv.Inclination.Value * 180.0 / Numeric.PI).ToString("F3", CultureInfo.InvariantCulture) + "\t" +
                                (sv.Azimuth.Value * 180.0 / Numeric.PI).ToString("F3", CultureInfo.InvariantCulture) + "\t\t" +
                                sv.TVD.Value.ToString("F3", CultureInfo.InvariantCulture) + "\t" +
                                sv.RiemannianNorth.Value.ToString("F3", CultureInfo.InvariantCulture) + "\t" +
                                sv.RiemannianEast.Value.ToString("F3", CultureInfo.InvariantCulture));
                        }
                    }
                }
            }
        }
    }
}
```
</pre>

The print out on the console looks like this:

<pre>
Calculated Trajectory
MD (m)  Incl (°)        Az (°)  TVD (m) Riem. North (m) Riem. East (m)  DLS (°/30m)     Toolface (°)    BUR (°/30m)     TUR (°/30m)
-39.700 0.000   0.000           -39.700 6560476.538     635467.313
10.300  0.900   -29.700         10.298  6560476.880     635467.119      0.540   90.000  0.540   -0.000
60.300  0.400   -95.100         60.295  6560477.205     635466.750      0.491   178.462 0.014   -70.337
110.300 0.700   142.500         110.294 6560476.947     635466.762      0.585   110.329 0.548   -16.631
160.300 0.900   58.200          160.290 6560476.912     635467.282      0.650   130.079 0.498   -26.657
210.300 0.600   173.200         210.288 6560476.859     635467.647      0.765   50.080  0.588   46.891
260.300 1.300   143.600         260.281 6560476.142     635468.014      0.500   110.873 0.467   -7.847
310.300 6.600   155.600         310.145 6560473.067     635469.539      3.201   87.087  3.197   1.415
360.300 11.200  143.700         359.534 6560466.533     635473.603      2.959   106.020 2.844   -4.203
Interpolated Trajectory
MD (m)  Incl (°)        Az (°)  TVD (m) Riem. North (m) Riem. East (m)  DLS (°/30m)     Toolface (°)    BUR (°/30m)     TUR (°/30m)
-39.700 0.000   0.000           -39.700 6560476.538     635467.313
-29.700 0.180   -29.700         -29.700 6560476.552     635467.305      0.540   -29.700 0.540   0.000
-19.700 0.360   -29.700         -19.700 6560476.593     635467.282      0.540   -29.700 0.540   0.000
-9.700  0.540   -29.700         -9.700  6560476.661     635467.243      0.540   -29.700 0.540   0.000
0.300   0.720   -29.700         0.299   6560476.757     635467.189      0.540   -29.700 0.540   0.000
10.300  0.900   -29.700         10.298  6560476.880     635467.119      0.540   -29.700 0.540   0.000
20.300  0.757   324.784         20.297  6560477.002     635467.042      0.491   -121.825        -0.417  -19.611
30.300  0.624   316.813         30.296  6560477.095     635466.966      0.491   -129.764        -0.377  -28.859
40.300  0.509   304.916         40.296  6560477.160     635466.893      0.491   -141.613        -0.305  -43.337
50.300  0.428   287.409         50.295  6560477.197     635466.821      0.491   -159.059        -0.175  -61.487
60.300  0.400   264.900         60.295  6560477.205     635466.750      0.491   178.462 0.014   -70.337
70.300  0.272   239.142         70.295  6560477.190     635466.695      0.585   -152.721        -0.266  -109.486
80.300  0.253   195.735         80.295  6560477.156     635466.669      0.585   163.932 0.164   -127.292
90.300  0.361   164.506         90.295  6560477.105     635466.671      0.585   132.489 0.432   -62.776
100.300 0.522   149.942         100.294 6560477.035     635466.702      0.585   117.815 0.517   -29.981
110.300 0.700   142.500         110.294 6560476.947     635466.762      0.585   110.329 0.548   -16.631
120.300 0.605   125.279         120.293 6560476.868     635466.843      0.650   -162.739        -0.192  -58.816
130.300 0.580   104.333         130.293 6560476.825     635466.935      0.650   176.333 0.043   -64.147
140.300 0.632   84.337          140.292 6560476.818     635467.039      0.650   156.305 0.262   -53.946
150.300 0.747   68.948          150.291 6560476.847     635467.154      0.650   140.866 0.411   -38.692
160.300 0.900   58.200          160.290 6560476.912     635467.282      0.650   130.079 0.498   -26.657
170.300 0.678   67.430          170.289 6560476.976     635467.403      0.765   -55.651 -0.631  36.482
180.300 0.490   84.581          180.289 6560477.003     635467.501      0.765   -38.614 -0.476  69.976
189.300 0.391   111.989         189.289 6560476.995     635467.567      0.765   -11.339 -0.148  109.964
190.300 0.387   115.700         190.289 6560476.992     635467.574      0.765   -7.636  -0.099  112.321
200.300 0.436   151.207         200.288 6560476.944     635467.622      0.765   27.948  0.360   88.902
210.300 0.600   173.200         210.288 6560476.859     635467.647      0.765   50.080  0.588   46.891
220.300 0.718   162.892         220.287 6560476.747     635467.671      0.500   130.221 0.382   -25.762
230.300 0.852   155.650         230.286 6560476.619     635467.720      0.500   122.956 0.419   -18.285
240.300 0.996   150.438         240.285 6560476.476     635467.794      0.500   117.728 0.442   -13.378
250.300 1.146   146.565         250.283 6560476.317     635467.892      0.500   113.845 0.457   -10.101
260.300 1.300   143.600         260.281 6560476.142     635468.014      0.500   110.873 0.467   -7.847
270.300 2.347   150.311         270.276 6560475.873     635468.183      3.201   81.778  3.168   11.177
280.300 3.407   152.868         280.263 6560475.431     635468.420      3.201   84.352  3.186   5.302
290.300 4.470   154.211         290.239 6560474.816     635468.725      3.201   85.699  3.192   3.080
300.300 5.535   155.039         300.201 6560474.027     635469.098      3.201   86.527  3.195   2.010
310.300 6.600   155.600         310.145 6560473.067     635469.539      3.201   87.087  3.197   1.415
320.300 7.487   152.071         320.069 6560471.968     635470.082      2.959   114.301 2.697   -9.345
330.300 8.395   149.294         329.974 6560470.765     635470.760      2.959   111.544 2.752   -7.441
340.300 9.320   147.059         339.854 6560469.457     635471.573      2.959   109.332 2.792   -6.048
350.300 10.256  145.227         349.709 6560468.047     635472.521      2.959   107.523 2.821   -5.003
360.300 11.200  143.700         359.534 6560466.533     635473.603      2.959   106.020 2.844   -4.203
</pre>

# SurveyStation

# SurveyStationList

## Realization of a SurveyStationList
A `SurveyStationList` has covariance matrices for each of its `SurveyStation`, meaning that a true trajectory can be anywhere in the surrounding of the
`SurveyStationList`. Yet, the covariance matrices correspond mostly to systematic errors that are probagated all along the series of measurements. So a realized
trajectory must respect a consistency compared to those covariance matrices. The method `Realize` produces a `SurveyList`, i.e., a list of `Survey`. There is
indeed no needs anymore to have information about the covariances in the realized trajectory. The generation algorithm is the following:

1. The last `SurveyStation` of the `SurveyStationList` is used to draw randomly a point according to the probability distribution associated with this `SurveyStation`.
For that purpose, the covariance matrix is diagonalized and the principal components are calculated. The eigenvalues are the variances in the three principal
directions,$${\sigma_x}^2, {\sigma_y}^2, {\sigma_z}^2$$, with $$x, y, z$$ being the local coordinate system along the principal directions. 
Three Gaussian probability distributions are created with zero mean and a variance equal to the eigen values, 
$$\mathcal{N}(0,{\sigma_x}^2),  \mathcal{N}(0,{\sigma_y}^2, \mathcal{N}(0,{\sigma_z}^2)$$. Three values are drawn using these probability distributions,
$$\hat{x}, \hat{y}, \hat{z}$$. The $$\chi^2_3$$ corresponding to this position is calculated using the following relation: 
$${\frac{\hat{x}^2}{{\sigma_x}^2}+\frac{\hat{y}^2}{{\sigma_y}^2}+\frac{\hat{z}^2}{{\sigma_z}^2}}={\chi^2_3}$$. The calculated $$\chi^2_3$$
is related to the confidence factor that the true `Survey` is within the ellipsoid delineated by $$\chi^2_3$$. The latitude and longitude
of that point are calculated using an instance of `SphericalPoint3D`. They are denoted respectively $$\phi_0$$ and $$\lambda_0$$. The 
randomly drawn point around the `SurveyStation` is then converted to a `Survey` in the Riemaniann manifold representing the Earth, using
the inverse transformation based on the eigenvectors.
2. Iteratively and in the upward direction, other `Station` are calculated using $$\phi_0$$ and $$\lambda_0$$ and a radial distance 
calculated using the ellipsoid of uncertainty defined by $$\chi^2_3$$, i.e., 
$$r^2=\frac{\chi^2_3}{\frac{\cos{\phi_0}^2.\cos{\lambda_0}^2}{{\sigma_x}^2}+\frac{\cos{\phi_0}^2.\sin{\lambda_0}^2}{{\sigma_y}^2}+\frac{\sin{\phi_0}^2}{{\sigma_z}^2}}$$
Of course, this `SphericalPoint3D` is defined in the local coordinate system directed by the principal components of the covariance
matrix of the `SurveyStation`. Having retrieved the $$x$$, $$y$$ and $$z$$ components of the point in the local coordinate system,
it is transformed to the Riemannian manifold coordinates using the inverse transformation based on the eigen vectors of the covariance
matrix. This operation generates a list of `Survey` for which the `RiemaniannNorth`, `RiemaniannEast` and `TVD` are filled in.
3. The last operation consists in calculating the `Inclination`, `Azimuth` and `Abscissa` at each `Survey`. From top to bottom, the list
is transversed and a circular arcj is calculated that links the previous `Survey`, which is fully defined, with the current `Survey`, 
which is only known by its `RiemaniannNorth`, `RiemaniannEast` and `TVD`. Knowing the circular arc, it is the possible to calculate the
length of the arc, i.e., derive the `Abscissa`, the `Inclination` and the `Azimuth`.

