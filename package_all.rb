#!/usr/bin/env ruby
# frozen_string_literal: true

require 'English'
require 'fileutils'

# Packages all mods into a zip file for upload to Github.
# Run only after running "export_all.rb"

ZIP_NAME = 'mods.zip'
ZIP_FOLDER = 'builds'
ZIP_FILE = "#{ZIP_FOLDER}/#{ZIP_NAME}"

EXTRA_PACKAGED_FILES = ['LICENSE', 'assets_license.txt']

def check_command_status
  return if $CHILD_STATUS.exitstatus.zero?

  puts 'Command failed to run'
  exit 2
end

def detect_subfolders
  Dir['*'].select { |o| File.directory?(o) }
end

if File.exist? ZIP_FILE
  puts 'Removing existing .zip'
  File.unlink ZIP_FILE
end

EXTRA_PACKAGED_FILES.each do |name|
  FileUtils.cp name, "#{ZIP_FOLDER}/#{name}"
end

Dir.chdir(ZIP_FOLDER) do
  mods = detect_subfolders
  puts 'Creating .zip...'
  system 'zip', '-r9', ZIP_NAME, *mods, *EXTRA_PACKAGED_FILES
  check_command_status

  puts "Packaged following mods: #{mods}"
end

puts "Created #{ZIP_FILE}"
