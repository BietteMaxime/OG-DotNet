The reference implementation of a C# client for the OpenGamma api http://www.opengamma.com/

Projects:
OGDotNet - The reference implementation of a C# client for the OpenGamma api http://www.opengamma.com/
OGDotNet.AnalyticsViewer - A simple results client using OGDotNet
OGDotNet.SecurityViewer - A security viewer using OGDotNet.  It also allows you to see time series data
OGDotNet.Tests - The unit tests for OGDotNet.  These tests should not need any other components
OGDotNet.Tests.Integration - The integration tests for OGDotNet.  These tests should be run against an OpenGamma server.  They may require specific configuration of that server in order to pass.  These also provide simple code snippets for using OGDotNet in maningful ways.

Other documents:
http://docs.opengamma.com/display/DOC/Writing+A+C+Sharp+Client - a basic introduction to OGDotNet

Configuring the sample apps:
The app.config file should be changed to point to your OpenGamma server.  They are in standard Castle http://www.castleproject.org/ format.

Configuring the integration tests:
The Settings.settings file should be changed to point to your OpenGamma server



