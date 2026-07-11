Project for Getting and Cleaning Data
===============================
https://github.com/fastzhong/GettingAndCleaningData

Refer to the course web site (https://class.coursera.org/getdata-008/) for the project details

Files
------
codebook.html: generated codebook (html)  
codebook.md: generated codebook (markdown)  
DatasetHumanActivityRecognitionUsingSmartphones.txt: tidy dataset file   
makeCodebook.Rmd: R markdown to produce codebook file   
README.md: this file  
run_analysis.html: generated run_analysis (html)  
run_analysis.md: generated run_analysis (markdown)  
run_analysis.r: the script to invoke run_analysis.Rmd  
run_analysis.Rmd: R markdown to produce the tidy dataset and also invoke the codebook R markdown to produce codebook  

UCI HAR Dataset: folder contains the project data (from https://d396qusza40orc.cloudfront.net/getdata%2Fprojectfiles%2FUCI%20HAR%20Dataset.zip)

Steps to reproduce this project
------------------------------------------
Prerequisite: clone this github repository to local 

1. Open the R script `run_analysis.r` using a text editor.
2. Change the parameter of the `setwd` function call to current working directory/folder and save.
3. Run the R script `run_analysis.r`. It calls the R Markdown file, `run_analysis.Rmd`, which contains the bulk of the code.

Outputs produced
-------------------------
* Tidy dataset file `DatasetHumanActivityRecognitionUsingSmartphones.txt` (tab-delimited text)
* Codebook file `codebook.md` (markdown) & `codebook.html` (html)
