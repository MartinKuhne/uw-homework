require(knitr)
require(markdown)
# change to your current folder accordingly
setwd("~/Copy/workspace/mooc_jhudatascience/datasciencecoursera/GettingAndCleaningData")
knit("run_analysis.Rmd", encoding="ISO8859-1")
markdownToHTML("run_analysis.md", "run_analysis.html")
