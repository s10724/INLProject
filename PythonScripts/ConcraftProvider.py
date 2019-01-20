import sys
import morfeusz2
import json
import os
import requests
import time
import collections
from itertools import groupby
from subprocess import Popen, PIPE
from collections import defaultdict
from operator import itemgetter

class Concraft(object):
    def __init__(self):
        self.http = 'http://localhost:3000/parse'
        loaded = False
        while not loaded:
            try:
                request_data = {'dag' : ''}
                r = requests.post(self.http, data=json.dumps(request_data))
                loaded = True
            except requests.ConnectionError as e:
                time.sleep(1)
   
    def tag(self, dag):
        json_string = ''
        for item in dag:
            i, j, (orth, base, tag, posp, kwal) = item
            line = u'{}\t{}\t{}\t{}\t{}\t{}\t{}\t0.0000\t\t\n'.format(i, j, orth, base, tag, ','.join(posp), ','.join(kwal))
            json_string += line
        analyse_list = []
        if json_string != '':
            request_data = {'dag' : json_string + '\n'}
            r = requests.post(self.http, data=json.dumps(request_data))                          
            for line in r.json()['dag'].split('\n'):
                if line != '':
                    i, j, orth, base, tag, posp, kwal, prob, interp, eos, disamb = line.strip('\n').split('\t')
                    posp = posp.split(',') if posp else []
                    kwal = kwal.split(',') if kwal else []
                    interp = ''
                    analyse_list.append((int(i), int(j), (orth, base, tag, posp, kwal), float(prob), eos, disamb))
                      
        return analyse_list


def skip_first(seq):
    it = iter(seq)
    next(it)
    return list(it)

tager = Concraft()
morf = morfeusz2.Morfeusz(generate=False, expand_tags=True)
rw = ''
if(len(sys.argv) > 0):
    rw = sys.argv[len(sys.argv)-1]

report = morf.analyse(rw)
tagged = tager.tag(report)
tagged= sorted(tagged, key=lambda x: (x[0], -x[3]))
actual = -1
for item in tagged:
    i, j, (orth, base, tag, posp, kwal), prob, eos, disamb = item

    if(actual != i):
       actual = i
       if(orth[0].isupper()):
            cap = 'W'
       else:
            cap = 'O'
       splitted = tag.split(':')
       if(len(splitted) == 0):
           ctag = 'O'
       else:
           ctag = splitted[0]

       if(len(splitted) < 2):
           msd = 'O'
       else:
           s=':'
           seq = skip_first(splitted)
           msd = s.join(seq)
           

       
       print(orth, base, cap, ctag, msd)
       if(eos):
            print()
    

