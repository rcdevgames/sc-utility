import 'package:flutter/cupertino.dart';
import 'package:flutter/material.dart';
import 'package:sc_utility/api/GithubApiClient.dart';
import 'package:sc_utility/utils/flutterextentions.dart';
import 'package:url_launcher/url_launcher.dart';
import '../resources.dart';
import '../translationProvider.dart';

class SettingsPage extends StatefulWidget {
  @override
  SettingsState createState() => SettingsState();
}

class SettingsState extends State<SettingsPage> {
  Resources resources;

  bool notificationsOn = false;
  bool nightModeOn = false;
  int nightModeState = 2;
  int language = 0;
  bool isLoading = false;

  @override
  void initState() {
    resources = Resources.getInstance();
    nightModeState = resources.prefs.getInt("themeMode") ?? 2;
    notificationsOn = resources.prefs.getBool("notifications") ?? true;
    language = resources.language();

    super.initState();
  }

  void onNotificationsChanged() {
    if (notificationsOn) {
      resources.prefs.setBool("notifications", true);
      resources.firebaseMessaging.subscribeToTopic("everyone");
    } else {
      resources.prefs.setBool("notifications", false);
      resources.firebaseMessaging.unsubscribeFromTopic("everyone");
    }
  }

  void onLanguageChanged(int value) {
    setState(() {
      language = value;
      resources.prefs.setInt("language", value);
    });
  }

  void handleRadioValueChanged(int value) {
    setState(() {
      nightModeState = value;
      resources.prefs.setInt("themeMode", value);

      switch (value) {
        case 0:
          {
            resources.myApp.updateThemeMode(ThemeMode.light);
            break;
          }

        case 1:
          {
            resources.myApp.updateThemeMode(ThemeMode.dark);
            break;
          }

        case 2:
          {
            resources.myApp.updateThemeMode(ThemeMode.system);
            break;
          }
      }
    });
  }

  @override
  Widget build(BuildContext context) {
    var version = resources.packageInfo.version;
    var build = resources.packageInfo.buildNumber;

    return Scaffold(
        appBar: AppBar(
          title: Text(TranslationProvider.get("TID_SETTINGS")),
        ),
        body: Stack(children: <Widget>[
          Column(
            children: [
              Flexible(
                fit: FlexFit.tight,
                child: ListView(
                  children: <Widget>[
                    ListTile(
                      title: Text(TranslationProvider.get("TID_NOTIFICATIONS")),
                    ),
                    Container(
                      child: ListTile(
                        onTap: () => {
                          setState(() {
                            notificationsOn = !notificationsOn;
                            onNotificationsChanged();
                          })
                        },
                        leading: notificationsOn
                            ? Icon(Icons.notifications_active)
                            : Icon(Icons.notifications_off),
                        title: Text(
                          TranslationProvider.get(
                              "TID_MAINTENANCE_NOTIFICATIONS"),
                        ),
                        subtitle: Text(TranslationProvider.get(
                            "TID_MAINTENANCE_NOTIFICATION_DESC")),
                        trailing: Switch(
                          value: notificationsOn,
                          onChanged: (value) {
                            setState(() {
                              notificationsOn = value;
                              onNotificationsChanged();
                            });
                          },
                          activeTrackColor: Colors.blueGrey[600],
                          activeColor: Colors.blueGrey[800],
                        ),
                      ),
                    ),
                    Divider(),
                    ListTile(
                        leading: Icon(Icons.style),
                        title: Text(
                          TranslationProvider.get("TID_THEME"),
                        )),
                    Center(
                      child: Wrap(
                        children: [
                          ChoiceChip(
                            selectedColor: Theme.of(context).accentColor,
                            labelStyle: TextStyle(color: Colors.white),
                            label: Text(TranslationProvider.get("TID_LIGHT")),
                            selected: nightModeState == 0,
                            onSelected: (value) {
                              handleRadioValueChanged(0);
                            },
                          ),
                          const SizedBox(width: 10),
                          ChoiceChip(
                            selectedColor: Theme.of(context).accentColor,
                            labelStyle: TextStyle(
                                color: nightModeState > 0
                                    ? Colors.white
                                    : Colors.black),
                            label: Text(TranslationProvider.get("TID_DARK")),
                            selected: nightModeState == 1,
                            onSelected: (value) {
                              setState(() {
                                handleRadioValueChanged(1);
                              });
                            },
                          ),
                          const SizedBox(width: 10),
                          ChoiceChip(
                            selectedColor: Theme.of(context).accentColor,
                            labelStyle: TextStyle(
                                color: nightModeState > 0
                                    ? Colors.white
                                    : Colors.black),
                            label: Text("System"),
                            selected: nightModeState == 2,
                            onSelected: (value) {
                              setState(() {
                                handleRadioValueChanged(2);
                              });
                            },
                          ),
                        ],
                      ),
                    ),
                    Divider(),
                    ListTile(
                        leading: Icon(Icons.language),
                        title: Text(TranslationProvider.get("TID_LANGUAGE"))),
                    Row(
                      mainAxisAlignment: MainAxisAlignment.center,
                      children: <Widget>[
                        flagButton(0, "assets/uk.png", language == 0),
                        flagButton(1, "assets/de.png", language == 1),
                      ],
                    ),
                    Divider(),
                    ListTile(
                      leading: Icon(Icons.info),
                      title: Text(
                        "Info",
                      ),
                      subtitle: Text("Version: $version Build: $build"),
                      trailing: IconButton(
                        icon: Icon(Icons.update),
                        onPressed: () async {
                          setState(() {
                            isLoading = true;
                          });

                          var isUpdateAvailable =
                              await GithubApiClient.isNewTagAvailable(resources
                                  .packageInfo.version
                                  .replaceAll(".debug", ""));

                          setState(() {
                            isLoading = false;
                          });

                          if (isUpdateAvailable) {
                            FlutterExtensions
                                .showPopupDialogWithActionAndCancel(
                                    context,
                                    TranslationProvider.get(
                                        "TID_UPDATE_AVAILABLE"),
                                    TranslationProvider.get(
                                        "TID_UPDATE_AVAILABLE_DESC"),
                                    TranslationProvider.get("TID_DOWNLOAD"),
                                    () => {
                                          launchURL(
                                              "https://github.com/Incr3dible/sc-utility/releases")
                                        },
                                    false);
                          } else {
                            FlutterExtensions.showPopupDialog(
                                context,
                                TranslationProvider.get("TID_UP_TO_DATE"),
                                TranslationProvider.get("TID_LATEST_VERSION"));
                          }
                        },
                      ),
                    ),
                  ],
                ),
              ),
              Container(
                margin: EdgeInsets.all(20),
                child: Center(
                    child: Text(
                  "Built with ❤",
                  style: TextStyle(fontSize: 14, fontWeight: FontWeight.bold),
                )),
              )
            ],
          ),
          isLoading
              ? Container(
                  color: Colors.black54,
                  child: Center(
                    child: CircularProgressIndicator(),
                  ),
                )
              : SizedBox.shrink()
        ]));
  }

  void launchURL(String url) async {
    if (await canLaunch(url)) {
      await launch(url);
    } else {
      throw 'Could not launch $url';
    }
  }

  Widget flagButton(int index, String assetImage, bool selected) {
    return Stack(
      children: <Widget>[
        Container(
          child: GestureDetector(
            onTap: () {
              onLanguageChanged(index);
            },
          ),
          margin: EdgeInsets.only(left: 10, right: 10, bottom: 10),
          width: 45,
          height: 30,
          decoration: BoxDecoration(
              image: DecorationImage(
                  image: AssetImage(assetImage), fit: BoxFit.fill),
              borderRadius: BorderRadius.circular(6)),
        ),
        selected
            ? Positioned.fill(
                child: Align(
                alignment: Alignment.bottomRight,
                child: Container(
                  child: Icon(
                    Icons.done,
                    size: 20,
                    color: Colors.white,
                  ),
                  decoration: BoxDecoration(
                    shape: BoxShape.circle,
                    color: Theme.of(context).accentColor,
                  ),
                ),
              ))
            : SizedBox.shrink()
      ],
    );
  }
}