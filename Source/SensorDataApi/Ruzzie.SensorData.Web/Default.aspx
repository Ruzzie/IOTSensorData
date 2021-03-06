﻿<%@ Page Language="C#"  %>
<%   
    string url = Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath.TrimEnd('/') + "/";
 %>
<!DOCTYPE html>
<html>
    <head>
        <title>Ruzzie's iot data platform</title>
        <link href='swagger/ui/css/typography-css' media='screen' rel='stylesheet' type='text/css'/>
        <link href='swagger/ui/css/reset-css' media='screen' rel='stylesheet' type='text/css'/>
        <link href='swagger/ui/css/screen-css' media='screen' rel='stylesheet' type='text/css'/>
        <link href='swagger/ui/css/reset-css' media='print' rel='stylesheet' type='text/css'/>
        <link href='swagger/ui/css/screen-css' media='print' rel='stylesheet' type='text/css'/>
  
        <script type="text/javascript" src="swagger/ui/lib/shred-bundle-js"></script>
        <script src='swagger/ui/lib/jquery-1-8-0-min-js' type='text/javascript'></script>
        <script src='swagger/ui/lib/jquery-slideto-min-js' type='text/javascript'></script>
        <script src='swagger/ui/lib/jquery-wiggle-min-js' type='text/javascript'></script>
        <script src='swagger/ui/lib/jquery-ba-bbq-min-js' type='text/javascript'></script>
        <script src='swagger/ui/lib/handlebars-2-0-0-js' type='text/javascript'></script>
        <script src='swagger/ui/lib/underscore-min-js' type='text/javascript'></script>
        <script src='swagger/ui/lib/backbone-min-js' type='text/javascript'></script>
        <script src='swagger/ui/lib/swagger-client-js' type='text/javascript'></script>
        <script src='swagger/ui/swagger-ui-js' type='text/javascript'></script>
        <script src='swagger/ui/lib/highlight-7-3-pack-js' type='text/javascript'></script>
        <script src='swagger/ui/lib/marked-js' type='text/javascript'></script>

        <!-- enabling this will enable oauth2 implicit scope support -->
        <script src='swagger/ui/lib/swagger-oauth-js' type='text/javascript'></script>
        <script type="text/javascript">
            $(function () {
                var url = window.location.search.match(/url=([^&]+)/);
                if (url && url.length > 1) {
                    url = decodeURIComponent(url[1]);
                } else {
                    url = "http://petstore.swagger.io/v2/swagger.json";
                }

                // Get Swashbuckle config into JavaScript
                function arrayFrom(configString) {
                    return (configString !== "") ? configString.split('|') : [];
                }

                function stringOrNullFrom(configString) {
                    return (configString !== "null") ? configString : null;
                }

                window.swashbuckleConfig = {
                    rootUrl: '<%=url%>',
                    discoveryPaths: arrayFrom('docs/v1'),
                    booleanValues: arrayFrom('true|false'),
                    validatorUrl: stringOrNullFrom(''),
                    customScripts: arrayFrom(''),
                    docExpansion: 'none',
                    oAuth2Enabled: Boolean('false'),
                    oAuth2ClientId: '',
                    oAuth2Realm: '',
                    oAuth2AppName: ''
                };

                window.swaggerUi = new SwaggerUi({
                    url: swashbuckleConfig.rootUrl + swashbuckleConfig.discoveryPaths[0],
                    dom_id: "swagger-ui-container",
                    booleanValues: swashbuckleConfig.booleanValues,
                    onComplete: function (swaggerApi, swaggerUi) {
                        if (typeof initOAuth == "function" && swashbuckleConfig.oAuth2Enabled) {
                            initOAuth({
                                clientId: swashbuckleConfig.oAuth2ClientId,
                                realm: swashbuckleConfig.oAuth2Realm,
                                appName: swashbuckleConfig.oAuth2AppName
                            });
                        }
                        $('pre code').each(function (i, e) {
                            hljs.highlightBlock(e)
                        });

                        window.swaggerApi = swaggerApi;
                        _.each(swashbuckleConfig.customScripts, function (script) {
                            $.getScript(script);
                        });
                    },
                    onFailure: function (data) {
                        log("Unable to Load SwaggerUI");
                    },
                    docExpansion: swashbuckleConfig.docExpansion,
                    sorter: "alpha"
                });

                if (window.swashbuckleConfig.validatorUrl !== '')
                    window.swaggerUi.options.validatorUrl = window.swashbuckleConfig.validatorUrl;

                function addApiKeyAuthorization() {
                    var key = $('#input_apiKey')[0].value;
                    log("key: " + key);
                    if (key && key.trim() != "") {
                        log("added key " + key);
                        window.authorizations.add("api_key", new ApiKeyAuthorization("api_key", key, "query"));
                    }
                }

                $('#input_apiKey').change(function () {
                    addApiKeyAuthorization();
                });

                // if you have an apiKey you would like to pre-populate on the page for demonstration purposes...
                /*
            var apiKey = "myApiKeyXXXX123456789";
            $('#input_apiKey').val(apiKey);
            addApiKeyAuthorization();
          */

                window.swaggerUi.load();
            });
        </script>
    </head>

    <body class="swagger-section">
                             
        <div id='header'>
            <div class="swagger-ui-wrap">
                <h1>Simple data store API for IOT</h1>
                <a id="logo" href="http://swagger.io">swagger</a>
                <form id='api_selector'>
                    <div class='input'><input placeholder="http://example.com/api" id="input_baseUrl" name="baseUrl" type="text"/></div>
                    <div class='input'><input placeholder="api_key" id="input_apiKey" name="apiKey" type="text"/></div>
                    <div class='input'><a id="explore" href="#">Explore</a></div>
                </form>
            </div>
           
        </div>
      

        <div id="message-bar" class="swagger-ui-wrap">&nbsp;</div>
        <div id="swagger-ui-container" class="swagger-ui-wrap"></div>
    </body>
</html>
