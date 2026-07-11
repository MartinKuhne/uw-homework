# plot1.R
# Author: Martin Kuhne
# Summary: plot for week 1 assignment
# cf. https://class.coursera.org/exdata-007/

source("data.R")

data = getEnergyData()

png(file = "plot1.png")
hist(data$GlobalActivePower, col = "red", 
     main = "Global Active Power",
     xlab = "Global Active Power (kilowatts)")
dev.off()
