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
	
def parse_twine_file(filepath):
    """Return a dictionary of data from the parsed file at filepath"""

    xml = ETree.parse(filepath)
    data = dict()
    parse_twine_tag(xml.getroot(), data)
    return data

if __name__ == "__main__":
	# Sample test
	inpath = r'Sample Data\TEST1.html'
	outpath = r'Sample Data\FinalOutput.json'
	data = parse_twine_file(inpath)
	with open(outpath, 'w') as f:
		dump(data, f, indent=4)
	dump("lololol", r'Sample Data\testOutput.json', indent=4)