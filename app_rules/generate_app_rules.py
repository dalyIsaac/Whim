from pathlib import Path
from requests import get
import yaml

# path to generated file
ROOT = Path(__file__).parents[0]
OUTFILE = ROOT / "DefaultFilteredWindows.cs"

# url of komorebi application rules
URL = "https://raw.githubusercontent.com/LGUG2Z/komorebi-application-specific-configuration/master/applications.yaml"

# portion of file above auto-generated rules
HEADER = """\
/* This file was generated from data with the following license:
 *
 * MIT License
 *
 * Copyright (c) 2021 Jade Iqbal
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

namespace Whim;

/// <summary>
/// Defaults for various <see cref="IFilterManager"/>s.
/// </summary>
public static class DefaultFilteredWindows
{
	/// <summary>
	/// Load the windows which should be ignored by Whim by default.
	/// </summary>
	/// <param name="filterManager"></param>
	public static void LoadWindowsIgnoredByWhim(IFilterManager filterManager)
	{
		filterManager.AddProcessFileNameFilter("SearchUI.exe");

		/// Auto-generated rules
"""

# portion of file below auto-generated rules
FOOTER = """\
	}

	/// <summary>
	/// Load the windows which try to set their own locations when the start up.
	/// See <see cref="IWindowManager.LocationRestoringFilterManager"/>
	/// </summary>
	/// <param name="filterManager"></param>
	public static void LoadLocationRestoringWindows(IFilterManager filterManager) =>
		filterManager.AddProcessFileNameFilter("firefox.exe").AddProcessFileNameFilter("gateway64.exe");
}
"""

# config options
TAB = "\t" * 2  # indention of of auto-generated rules, " " * 8 or "\t" * 2
COMMENT = "// "  # comment string used for auto-generated content, "// " or "/// "


class GetRules:
	def __init__(self, url):
		self.url = url
		self.out = ROOT / "komorebi_rules.yaml"
		self.rules = None

	def download(self):
		response = get(self.url)
		with open(self.out, "wb") as f:
			f.write(response.content)

	def load_yaml(self):
		with open(self.out, "r") as f:
			self.rules = yaml.safe_load(f)


class GenerateRules:
	def __init__(self, komorebi_rules):
		self.komorebi_rules = komorebi_rules

	def generate_all_rules(self):
		for app in self.komorebi_rules:
			if "float_identifiers" in app:
				Application(app["float_identifiers"], app["name"]).generate_rules()


class Application:
	def __init__(self, app_rules, app_name):
		self.app_name = app_name
		self.app_rules = app_rules

	def generate_rules(self):
		with open(OUTFILE, 'a') as o:
			o.write("".join(["\n", TAB, COMMENT, self.app_name, "\n"]))
		for r in self.app_rules:
			FloatRule(r).add_rule()


class FloatRule:
	def __init__(self, rule):
		self.kind = rule["kind"]
		self.id = rule["id"]
		self.matching_strategy = rule[_] if (_ := "matching_strategy") in rule else None
		self.comment = rule[_] if (_ := "comment") in rule else None

		# Raise error for unsupported matching strategies
		# If future rules use regex, we can implement them via "AddTitleMatchFilter"
		if self.matching_strategy and self.matching_strategy != "Equals":
			raise RuntimeError('Matching strategy "{_}" unsupported')

	def add_rule(self):
		match self.kind:
			case "Class":
				command = "AddWindowClassFilter"
			case "Exe":
				command = "AddProcessFileNameFilter"
			case "Title":
				command = "AddTitleFilter"
			case _:
				raise RuntimeError("Undefined kind: " + self.kind)

		content = "".join(["filterManager.", command, '("', self.id, '");'])
		comment = "  // " + self.comment if self.comment else ""
		if self.id in _processed[self.kind]:  # duplicate rule
			with open(OUTFILE, 'a') as o:
				o.write("".join([TAB, "// ", content, "  // duplicate rule\n"]))
		else:								  # new rule
			_processed[self.kind] += [self.id]
			with open(OUTFILE, 'a') as o:
				o.write("".join([TAB, content, comment, "\n"]))


# Add header
with open(OUTFILE, 'w') as o:
	o.write(HEADER)

# Keep track of already generated rules to filter out duplicates
_processed = {"Class": [], "Exe": [], "Title": []}

# Load komorebi rules
komorebi_rules = GetRules(URL)
komorebi_rules.download()
komorebi_rules.load_yaml()

# Generate rules
GenerateRules(komorebi_rules.rules).generate_all_rules()

# Add footer
with open(OUTFILE, 'a') as o:
	o.write(FOOTER)

# vim: set ts=4 sw=4 noet :
