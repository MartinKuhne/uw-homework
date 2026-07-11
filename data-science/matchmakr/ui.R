# ui.R
# Author: Martin Kuhne
# Summary: UI for matchmakr course project
# cf. https://class.coursera.org/devdataprod-006/

library(shiny)

shinyUI(pageWithSidebar(

    headerPanel("matchmakr"),
    
    # Sidebar with a slider input for number of observations
    sidebarPanel(
        h3("About you"),
        
        selectInput("interest1", "Main interest", 
                     choices = c("pick..." = 0, "arts" = 1, "sports" = 2, "drinking" = 3)),
        
        selectInput("reward1", "Feeling rewarded by", 
                    choices = c("pick..." = 0, "money" = 1, "recognition" = 2, "friendship" = 3)),

        selectInput("location1", "Living", 
                    choices = c("pick..." = 0, "downtown" = 1, "suburb" = 2, "Alabama" = 3)),

        h3("About your romantic interest"),

        selectInput("interest2", "Main interest", 
                    choices = c("pick..." = 0, "arts" = 1, "sports" = 2, "drinking" = 3)),
    
        selectInput("reward2", "Feeling rewarded by", 
                    choices = c("pick..." = 0, "money" = 1, "recognition" = 2, "friendship" = 3)),
    
        selectInput("location2", "Living", 
                    choices = c("pick..." = 0, "downtown" = 1, "suburb" = 2, "Alabama" = 3))
    ),
        
    mainPanel(
        includeMarkdown("help.md"),
        h3("Your relationship rating"),
        h4(textOutput("rating"))
    )
))