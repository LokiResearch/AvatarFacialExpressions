# MIT License
#
# Copyright 2021 Marc Baloup, Thomas Pietrzak, Martin Hachet, Géry Casiez (Université de Lille, Inria, France)
#
# Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
#
# The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
#
# THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.


import time
import sys
import json
from os import path
import speech_recognition as sr

filePath = sys.argv[2]
lang = sys.argv[1]

output = open(filePath + ".out", "w")




try:
	r = sr.Recognizer()
	with sr.AudioFile(filePath) as source:
		audio = r.record(source)  # read the entire audio file
	find = r.recognize_google(audio, language=lang, show_all=True)
	findstr = json.dumps(find)
	if (findstr == "[]"):
		out_txt = "dont_understand"
	else:
		out_txt = findstr
except sr.RequestError as e:
	out_txt = "error: {0}".format(e)


output.write(out_txt)
output.close()
