#!/usr/bin/env ruby
# frozen_string_literal: true

require 'English'
require 'fileutils'

# This script exports all the mods and prepares distributable mod folders in "builds"
# folder

def check_command_status
  return if $CHILD_STATUS.exitstatus.zero?

  puts 'Command failed to run'
  exit 2
end

def export_project(folder, target, output)
  puts "Exporting mod #{folder}"
  Dir.chdir(folder) do
    system 'godot', '--export', target, output
    check_command_status
  end
end

def make_mod_folder(mod_name, pck, extra)
  puts "Packaging mod folder for #{mod_name}"

  target_path = File.join 'builds', mod_name

  FileUtils.mkdir_p target_path

  FileUtils.cp File.join(mod_name, pck), target_path

  extra.each do |extra_file|
    if extra_file.is_a? Array
      FileUtils.cp File.join(mod_name, extra_file[0]), File.join(target_path, extra_file[1])
    else
      FileUtils.cp_r File.join(mod_name, extra_file), target_path
    end
  end
end

def process_disco_nucleus
  export_project 'DiscoNucleus', 'Linux/X11', 'builds/DiscoNucleus.x86_64'
  make_mod_folder 'DiscoNucleus', 'builds/DiscoNucleus.pck',
                  ['thrive_mod.json', 'disco_icon.png']

  puts 'DiscoNucleus exported'
end

def process_all
  process_disco_nucleus
end

FileUtils.mkdir_p 'builds'

process_all
