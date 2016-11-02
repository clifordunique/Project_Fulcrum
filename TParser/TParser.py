import xml.etree.ElementTree as ETree
import os, stat
from json import dump


PASSAGE_TAG = "tw-passagedata"
MULTIPLE_TAG_ERROR = "Found multiple '{}' tags, not currently supported"

def parse_components(nodeid, id, taglist):

	left = 0
	emote = 0
	actor = -1
	text = "..."
	choice = 0
	exit = 0

	for tags in taglist:
		if tags == 'neutral':
			print("neutral")
			emote = 0
		elif tags == 'happy':
			print("happy")
			emote = 1
		elif tags == 'athena':
			print("athena")
			actor = 0
		elif tags == 'morris':
			print("morris")
			actor = 1
		elif tags == 'left':
			print("left")
			left = 1
		elif tags.startswith('>'):
			text = tags[1:]
			print(text)
		elif tags == 'exit':
			exit = 1
			print("exit")
		elif tags.startswith('[[') and tags.endswith(']]'):
			trim = tags[2:-2]
			splittrim = trim.split('->')
			text = splittrim[0]
			choice = 1
			if len(splittrim)>1:
				print(text+" - LINKING TO: "+splittrim[1])
			else:
				print(text)
		else:
			print("no match on tag: "+tags)
	
	dialogueDict = {
		'text':text,
		'actor':actor,
		'emote':emote,
		'leftSide':left,
		'exit':exit
	}
	choiceDict = {
		'text':text,
		'actor':actor,
		'emote':emote,
		'leftSide':left
	}
			
	if choice == 1:
		dpath = r'output/Node_'+str(nodeid)+'/Choice_'+str(id)+'/Dialogue_'+str(id)+r'.json'
	else:
		dpath = r'output/Node_'+str(nodeid)+'/Dialogue_'+str(id)+r'.json'

	ensure_dir(dpath)
	with open(dpath, 'w') as p:
		dump(dialogueDict, p, indent=4)

def ensure_dir(f):
    d = os.path.dirname(f)
    if not os.path.exists(d):
        os.makedirs(d)
		
def parse_dnode_file(filepath):
	"""Return a dictionary of data from the parsed file at filepath"""

	xml = ETree.parse(filepath)
	dialogues = dict()
	parse_dnodes(xml.getroot(), dialogues)
	return dialogues
		
def parse_dnodes(element, data):
	
	tagname = element.tag
	attributes = element.attrib
	if tagname == PASSAGE_TAG:
		nodeid = element.get('pid')
		dialogues = element.text.split("\n")
		i=1
		for lines in dialogues:
			components = lines.split("/")
			parse_components(nodeid, i, components)
			attributes['dialogue_'+str(i)] = components
			i+=1
			data.setdefault(PASSAGE_TAG, []).append(attributes)
	elif tagname in data:
		raise ValueError(MULTIPLE_TAG_ERROR.format(tagname))
	else:
		print('else')
		data[tagname] = attributes
	
	for child in element:
		print('end')
		parse_dnodes(child, data)
	

if __name__ == "__main__":
	inpath = r'TEST1.html'
	outpath = r'FinalOutput.json'
	altpath = r'altfile.json'
	
	os.chmod(inpath, stat.S_IWRITE)
	
	with open(inpath, 'r') as myfile:
		data=myfile.read().replace('" hidden', '"')
		
	with open(inpath, 'w') as myfile:	
		myfile.write(data)
	
	dnodeData = parse_dnode_file(inpath)

	with open(altpath, 'w') as n:
		dump(dnodeData, n, indent=4)
		
#OLD GARBAGE - REMOVE LATER

'''	
def parse_twine_file(filepath):
	"""Return a dictionary of data from the parsed file at filepath"""

	xml = ETree.parse(filepath)
	data = dict()
	parse_twine_tag(xml.getroot(), data)
	return data

	
def parse_twine_tag(element, data):
	
	tagname = element.tag
	attributes = element.attrib
	
	if tagname == PASSAGE_TAG:
		dialogues = element.text.split("\n")
		i = 0
		for lines in dialogues:
			components = lines.split("/")
			attributes['dialogue_'+str(i)] = components
			i+=1
			data.setdefault(PASSAGE_TAG, []).append(attributes)
	elif tagname in data:
		raise ValueError(MULTIPLE_TAG_ERROR.format(tagname))
	else:
		data[tagname] = attributes
	
	for child in element:
		parse_twine_tag(child, data)
'''
