# Secret Folder

This folder contains the secret keys to all the external resources ``Snuffo`` is using like ``Cloudinary``, ``Auth0`` etc. That's why it is not included in the repo.

To run this application locally make sure you have these appSettings configured:

```xml
<appSettings>
  <!--
    Go to cloudinary.com and signup for an account and enter your api configurations.
  -->
  <add key="Cloudinary.Name" value="" />
  <add key="Cloudinary.API.Key" value="" />
  <add key="Cloudinary.API.Secret" value="" />
  <add key="Cloudinary.Url" value="https://res.cloudinary.com/" />

  <!--
   (Optional if you don't want to sign in)
    Go to Auth0.com and signup for an account and enter your client configurations.
  -->
  <add key="auth0:Domain" value="" />
  <add key="auth0:ClientId" value="" />
  <add key="auth0:ClientSecret" value="" />

  <!--
    (Optional if you don't want to post anything)
    Go to https://www.google.com/recaptcha > Admin console > Settings and configure your keys.
  -->
  <add key="reCaptcha:SiteKey" value="" />
  <add key="reCaptcha:SecretKey" value="" />

  <!-- 
    These settings are optional. In case you have an IDEAL ING Bank account, you can specify these values.
    But this app can run without them.
  -->
  <add key="Privatecert" value="" />
  <add key="Acquirercert" value="" />
  <add key="FindCertificatesByThumbprint" value="False" />
  <add key="UseCspMachineKeyStore" value="False" />
  <add key="UseCertificateWithEnhancedAESCryptoProvider" value="False" />
  <add key="AcquirerURL" value="https://idealtest.secure-ing.com/ideal/iDEALv3" />
  <add key="AcquirerTimeout" value="10" />
  <add key="MerchantID" value="" />
  <add key="SubID" value="0" />
  <add key="MerchantReturnURL" value="" />
  <add key="ExpirationPeriod" value="PT30M" />
</appSettings>
```