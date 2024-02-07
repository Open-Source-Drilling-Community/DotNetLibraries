# Preamble
This package is developed as part of the Society of Petroleum (SPE) Open Source Drilling Community, a sub-committee of the Drilling System Automation Technical Section.
This package contains classes to perform survey calculations

# Conversion from Latitude-Longitude to X-Y

The earth is modelled as an oblate, i.e., a spheroid flatened at the pole. At a given latitude, a path on the Earth is a circle. Let us consider that the origin of 
longitudes is Greenwich and that the Earth is modelled by a semi-long axis, $a$, and a flatening, $f$. The flatening is defined as:
$f = \frac{{a - b}}{{a}}$
where $b$ is the semi-short axis. Therefore the semi short axis can be expressed as:
$b = a - f \cdot a$

The radius of the Earth at a given latitude, $\phi$ is given by:
$R(\phi) = \frac{{a \cdot \sqrt{{\cos^2(\phi) + \frac{{b^2}}{{a^2}} \cdot \sin^2(\phi)}}}}{{\sqrt{1 - f \cdot (2 - f) \cdot \sin^2(\phi)}}}$

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

At that latitude the $y$-coordinate (east-west) is the length of the circular arc counted from the Greenwich meridian, i.e., the longitude angle, $\lambda$:
$y = R(\phi) \cdot \lambda$
The $x$-coordinate (south-north) is the length of the elliptical arc counted from the equator using the latitude. 
This involves the elliptic integral of the second kind, denoted $E(\phi, m)$ where $m=1- \frac{{b^2}}{{a^2}}$. 
Its definition is: $E(\phi, m) = \int_0^\phi \sqrt{1 - m \cdot \sin^2(t)} \, dt$.
The definition of $x$ is then: $x = a \cdot E(\phi, m)$.

Conversely, to retrieve the latitude and longitude from the $x$ and $y$ coordinates, i.e., arc lengths, the following method is used:
$\phi = E^{-1}(\frac{x}{a}, m)$ and $\lambda = \frac{y}{R(\phi)}$.

The elliptic integral of the second kind is calculated using the special function defined in `OSDC.DotnetLibraries.General.Math`, 
namely `SpecialFunctions.EllipticE(phi, m)` and its inverse is `Elliptic.InverseEllipticE(x, m)`.

So in conclusion, the $x$ and $y$ coordinates of `CurvilinearPoint3D` are not coordinates on a line but arcs. $x$ is a circular arc and $y$ is an elliptical arc.
The origin of $x$ and $y$ is the point at the equator at the Greenwich meridian. Their calculations is based on the WGS84 definition of the Earth.


