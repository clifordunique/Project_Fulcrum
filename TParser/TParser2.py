import xml.etree.ElementTree as ETree

inpath = r'Sample Data\TwineInput.html'
xml = ETree.parse(inpath)
for element in xml.getroot():
    print(element.tag, element.attrib)