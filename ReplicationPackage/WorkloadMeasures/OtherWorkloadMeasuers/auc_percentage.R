install.packages("MESS")
library(MESS)

openA <- read.csv('~/Downloads/OpenRevs/OpenReviewsPerQuarter.csv', header = T)
openAuthRec <- read.csv('~/Downloads/OpenRevs/OpenReviewsAuthRecPerQuarter.csv', header = T)
openOwnRec <- read.csv('~/Downloads/OpenRevs/OpenReviewsOwnRecPerQuarter.csv', header = T)
opencH <- read.csv('~/Downloads/OpenRevs/OpenReviewscHRevPerQuarter.csv', header = T)
openLearnRec <- read.csv('~/Downloads/OpenRevs/OpenReviewsLearnRecPerQuarter.csv', header = T)
openRet <- read.csv('~/Downloads/OpenRevs/OpenReviewsRetentionRecPerQuarter.csv', header = T)
openTurnRec <- read.csv('~/Downloads/OpenRevs/OpenReviewsTurnRecPerQuarter.csv', header = T)
openSV1 <- read.csv('~/Downloads/OpenRevs/OpenReviewsSofiaV1PerQuarter.csv', header = T)
openWhoDo <- read.csv('~/Downloads/OpenRevs/OpenReviewsWhoDoPerQuarter.csv', header = T)
openSV2 <- read.csv('~/Downloads/OpenRevs/OpenReviewsSofiaV2PerQuarter.csv', header = T)

auc_difference_percentage <- function(a, s) {
  
  da <- percentage_percentage_freq(a);
  ds <- percentage_percentage_freq(s);
  
  #total area - doesn't makes sense as below even distribution is impossible given ranking and summing
  #percentage_difference(auc(da$percentage, da$cumulative, from = 1, to = 100), auc(ds$percentage, ds$cumulative, from = 1, to = 100))
  
  #relative getting rid of the area that is impossible to achieve based on cumulative distribution and rank (is a weighted avg)
  percentage_difference(auc(da$percentage, da$cumulative)-5000, auc(ds$percentage, ds$cumulative)-5000)
  
}

auc_difference_percentage_all <- function(a, s) {
  r <- c(auc_difference_percentage(a$CoreFX, s$CoreFX), 
         auc_difference_percentage(a$CoreCLR, s$CoreCLR),
         auc_difference_percentage(a$Roslyn, s$Roslyn),
         auc_difference_percentage(a$Rust, s$Rust),
         auc_difference_percentage(a$Kubernetes, s$Kubernetes)
  )
  print(mean(r))
  r
}

auc_difference_percentage_all(openA, openAuthRec)
auc_difference_percentage_all(openA, openOwnRec)
auc_difference_percentage_all(openA, opencH)
auc_difference_percentage_all(openA, openLearnRec)
auc_difference_percentage_all(openA, openRet)
auc_difference_percentage_all(openA, openTurnRec)
auc_difference_percentage_all(openA, openSV1)
auc_difference_percentage_all(openA, openWhoDo)
auc_difference_percentage_all(openA, openSV2)

