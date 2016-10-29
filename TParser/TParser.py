import xml.etree.ElementTree as ETree
from json import dump


PASSAGE_TAG = "tw-passagedata"
MULTIPLE_TAG_ERROR = "Found multiple '{}' tags, not currently supported"

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
		
def create_dialogue(nodeid, id, taglist):

	left = 0
	emote = 0
	actor = -1
	text = "..."

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
		else:
			print("no match on tag: "+tags)
				
	dpath = r'Dialogue_'+str(nodeid)+'_'+str(id)+r'.json'
	dict = {
			'text':text,
			'actor':actor,
			'emote':emote,
			'leftSide':left,
			}
	with open(dpath, 'w') as p:
		dump(dict, p, indent=4)
		
def parse_dnodes(element, data):
	
	tagname = element.tag
	attributes = element.attrib
	print("lil?")
	if tagname == PASSAGE_TAG:
		nodeid = element.get('pid')
		dialogues = element.text.split("\n")
		i=1
		for lines in dialogues:
			components = lines.split("/")
			create_dialogue(nodeid, i, components)
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
	
def parse_twine_file(filepath):
	"""Return a dictionary of data from the parsed file at filepath"""

	xml = ETree.parse(filepath)
	data = dict()
	parse_twine_tag(xml.getroot(), data)
	return data
	
def parse_dnode_file(filepath):
	"""Return a dictionary of data from the parsed file at filepath"""

	xml = ETree.parse(filepath)
	dialogues = dict()
	parse_dnodes(xml.getroot(), dialogues)
	return dialogues

if __name__ == "__main__":
	# Sample test
	print("lel")
	inpath = r'TEST1.html'
	outpath = r'FinalOutput.json'
	altpath = r'altfile.json'
	#data = parse_twine_file(inpath)
	dnodeData = parse_dnode_file(inpath)
	#with open(outpath, 'w') as f:
	#	dump(data, f, indent=4)
	with open(altpath, 'w') as n:
		dump(dnodeData, n, indent=4)
