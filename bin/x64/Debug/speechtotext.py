#!/usr/bin/python
# -*- coding: utf-8 -*-
import sys
import urllib2
import json
 
KEY = "AIzaSyCEU-Du89FOAfCKpWVYstWpmpnsLW2Q2eg"
URL = "https://www.google.com/speech-api/v2/recognize?lang=zh-TW&key=%s" % KEY
 
def speech_to_text(filename):
    audio = open(filename, "rb").read()
    content_type = "audio/x-flac; rate=44100;"
    if filename.lower().endswith(".wav"):
        content_type = "audio/l16; rate=16000;"
    header = {"Content-Type": content_type}
 
    request = urllib2.Request(URL, data=audio, headers=header)
    response = urllib2.urlopen(request)
    data = response.read()
    file = open('speech_to_text.txt', 'w')
    if data.split("\n") >= 2:
        if data.split("\n")[1].strip() != "":
            parser = json.loads(data.split("\n")[1])
            for text in parser["result"][0]["alternative"]:
                file.write(text["transcript"].encode('UTF-8') + "\n")
                #print text["transcript"]
                #if text.get("confidence"):
                 #   print text["confidence"]
        else:
            print "No data."
    else:
        print "No response."
 
if __name__ == "__main__":
    if len(sys.argv) >= 2:
        speech_to_text(sys.argv[1])