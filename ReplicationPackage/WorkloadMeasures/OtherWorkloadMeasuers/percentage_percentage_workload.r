# Install the required packages
install.packages("e1071")
library(e1071)    
install.packages("MESS")
library(MESS)



# Get the CSV files
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

#convience function
percentage_difference <- function(a, s) {
  ((s/a)-1) *100
}

#Calculate actual review concentration
#empirical result showing the tight concentration of top reviewers, ie 80-20 rule
plot_percentage_percentage(percentage_percentage_freq(openA$CoreFX))
plot_line_percentage_percentage(percentage_percentage_freq(openA$CoreCLR), lty = 2)
plot_line_percentage_percentage(percentage_percentage_freq(openA$Roslyn), lty = 3)
plot_line_percentage_percentage(percentage_percentage_freq(openA$Rust), lty = 4)
plot_line_percentage_percentage(percentage_percentage_freq(openA$Kubernetes), lty = 5)

percentage_difference_pp <- function(a, s, p) {
  (get_cumulative_at_df(percentage_percentage_freq(s), p)/get_cumulative_at_df(percentage_percentage_freq(a), p)-1) *100
}

percentage_difference_pp(openA$Kubernetes, openOwnRec$Kubernetes, 20)

#Simulation, how does the recommender change the 80-20 distribution?
percentage_difference_pp_all <- function(a, s, p) {
  r <- c(percentage_difference_pp(a$CoreFX, s$CoreFX, p), 
    percentage_difference_pp(a$CoreCLR, s$CoreCLR, p),
    percentage_difference_pp(a$Roslyn, s$Roslyn, p),
    percentage_difference_pp(a$Rust, s$Rust, p),
    percentage_difference_pp(a$Kubernetes, s$Kubernetes, p)
  )
  print(mean(r))
  r
}

get_y_cumulative_at_percentage(na.omit(openA$CoreFX), 80)


percentage_difference(auc(percentage_percentage_freq(openA$Kubernetes)$percentage, seq(1:length(na.omit(openA$Kubernetes)))),
  auc(percentage_percentage_freq(openWhoDo$Kubernetes)$percentage, seq(1:length(na.omit(openWhoDo$Kubernetes))))
)



auc_difference_percentage <- function(a, s) {

  
  da <- percentage_percentage_freq(a);
  ds <- percentage_percentage_freq(s);
  
  #total area
  #percentage_difference(auc(da$percentage, da$cumulative, from = 1, to = 100), auc(ds$percentage, ds$cumulative, from = 1, to = 100))
  
  #relative getting rid of the area that is impossible to achieve based on cumulative distribution and rank (is a weighted avg)
  percentage_difference(auc(da$percentage, da$cumulative)-5000, auc(ds$percentage, ds$cumulative)-5000)
  
  #normal wrt actual and simulated
  #db <- percentage_percentage_freq(openA$Kubernetes)  
  #percentage_difference(
  #                      auc(db$percentage, db$cumulative)-auc(ds$percentage, ds$cumulative),
  #                      auc(db$percentage, db$cumulative)- 5000
  #                      )
  
  #relative to the lowest one
  #db <- percentage_percentage_freq(openWhoDo$Kubernetes)  
  #percentage_difference(auc(da$percentage, da$cumulative)-auc(db$percentage, db$cumulative), 
  #  auc(ds$percentage, ds$cumulative)-auc(db$percentage, db$cumulative))

}

auc_total <- function(s) {
  
  ds <- percentage_percentage_freq(s);
  (auc(ds$percentage, ds$cumulative)-5000)/5000*100
}

auc_total(openOwnRec$Kubernetes) - auc_total(openA$Kubernetes)
auc_total(openWhoDo$Kubernetes) - auc_total(openA$Kubernetes)

auc_difference_percentage(openA$Kubernetes, openAuthRec$Kubernetes)
auc_difference_percentage(openA$Kubernetes, openOwnRec$Kubernetes)
auc_difference_percentage(openA$Kubernetes, opencH$Kubernetes)
auc_difference_percentage(openA$Kubernetes, openLearnRec$Kubernetes)
auc_difference_percentage(openA$Kubernetes, openRet$Kubernetes)
auc_difference_percentage(openA$Kubernetes, openTurnRec$Kubernetes)
auc_difference_percentage(openA$Kubernetes, openSV1$Kubernetes)
auc_difference_percentage(openA$Kubernetes, openWhoDo$Kubernetes)
auc_difference_percentage(openA$Kubernetes, openSV2$Kubernetes)

p = 20
percentage_difference_pp_all(openA, openAuthRec, p)
percentage_difference_pp_all(openA, openOwnRec, p)
percentage_difference_pp_all(openA, opencH, p)
percentage_difference_pp_all(openA, openLearnRec, p)
percentage_difference_pp_all(openA, openRet, p)
percentage_difference_pp_all(openA, openTurnRec, p)
percentage_difference_pp_all(openA, openSV1, p)
percentage_difference_pp_all(openA, openWhoDo, p)
percentage_difference_pp_all(openA, openSV2, p)


percentage_pp_all <- function(d, p) {
  r <- c(get_cumulative_at_df(percentage_percentage_freq(d$CoreFX), p), 
    get_cumulative_at_df(percentage_percentage_freq(d$CoreCLR), p), 
    get_cumulative_at_df(percentage_percentage_freq(d$Roslyn), p), 
    get_cumulative_at_df(percentage_percentage_freq(d$Rust), p), 
    get_cumulative_at_df(percentage_percentage_freq(d$Kubernetes), p)
  )
  print(mean(r))
  #print(100-mean(r))
  r
}

#raw value for pp
p = 20
percentage_pp_all(openA, p)
percentage_pp_all(openAuthRec, p)
percentage_pp_all(openOwnRec, p)
percentage_pp_all(opencH, p)
percentage_pp_all(openLearnRec, p)
percentage_pp_all(openRet, p)
percentage_pp_all(openTurnRec, p)
percentage_pp_all(openSV1, p)
percentage_pp_all(openWhoDo, p)
percentage_pp_all(openSV2, p)


#how does the percentile change?
percentile_difference_all <- function(a, s, p) {
  pd <- p/100
  r <- c(
    percentage_difference(quantile(a$CoreFX, pd, na.rm = T, names = F), quantile(s$CoreFX, pd, na.rm = T, names = F)),
    percentage_difference(quantile(a$CoreCLR, pd, na.rm = T, names = F), quantile(s$CoreCLR, pd, na.rm = T, names = F)),
    percentage_difference(quantile(a$Roslyn, pd, na.rm = T, names = F), quantile(s$Roslyn, pd, na.rm = T, names = F)),
    percentage_difference(quantile(a$Rust, pd, na.rm = T, names = F), quantile(s$Rust, pd, na.rm = T, names = F)),
    percentage_difference(quantile(a$Kubernetes, pd, na.rm = T, names = F), quantile(s$Kubernetes, pd, na.rm = T, names = F))
  )
  print(mean(r))
  r
  
}

pd = .5
percentage_difference(openA$Kubernetes, pd, na.rm = T, names = F)
quantile(openOwnRec$Kubernetes, pd, na.rm = T, names = F)

#percentile
p = 20
percentile_difference_all(openA, openAuthRec, p)
percentile_difference_all(openA, openOwnRec, p)
percentile_difference_all(openA, opencH, p)
percentile_difference_all(openA, openLearnRec, p)
percentile_difference_all(openA, openRet, p)
percentile_difference_all(openA, openTurnRec, p)
percentile_difference_all(openA, openSV1, p)
percentile_difference_all(openA, openWhoDo, p)
percentile_difference_all(openA, openSV2, p)


#What percentage of reviewers do 80% of the work? 

#plots for individual projects
plot(seq(1:100), seq(1:100), type = 'l', lwd = 3, lty = 1, ylab = "Cumulative Percentage of Reviews", xlab = "Percentage of Reviewers")
plot_line_percentage_percentage(percentage_percentage_freq(openA$Kubernetes), lty = 2)
plot_line_percentage_percentage(percentage_percentage_freq(openAuthRec$Kubernetes), lty = 1)
plot_line_percentage_percentage(percentage_percentage_freq(openOwnRec$Kubernetes), lty = 3)
plot_line_percentage_percentage(percentage_percentage_freq(opencH$Kubernetes), lty = 4)
plot_line_percentage_percentage(percentage_percentage_freq(openLearnRec$Kubernetes), lty = 4)
plot_line_percentage_percentage(percentage_percentage_freq(openRet$Kubernetes), lty = 3)
plot_line_percentage_percentage(percentage_percentage_freq(openSV1$Kubernetes), lty = 6)
plot_line_percentage_percentage(percentage_percentage_freq(openWhoDo$Kubernetes), lty = "dotted")
plot_line_percentage_percentage(percentage_percentage_freq(openSV2$Kubernetes), lty = 4)

pdf("~/Downloads/WorkloadAUCExample.pdf")
plot(seq(1:100), seq(1:100), type = 'l', lwd = 3, lty = "twodash", ylab = "Cumulative Percentage of Reviews", xlab = "Percentage of Reviewers")
plot_line_percentage_percentage(percentage_percentage_freq(openA$Kubernetes), lty = "solid")
plot_line_percentage_percentage(percentage_percentage_freq(openOwnRec$Kubernetes), lty = "dotted", pch = 2)
plot_line_percentage_percentage(percentage_percentage_freq(openWhoDo$Kubernetes), lty = "dashed", pch = 6)
abline(v = 20)
legend(50, 20, legend=c("Past Reviewers Rec", "Actual Workload", "Workload Aware Rec", "Perfectly Equal Workload"), lty=c('dotted', 'solid', 'dashed', 'twodash'), lwd = 3, cex=1.2)
dev.off()

pdf("~/Downloads/WorkloadAUCDiscussion.pdf")
plot(seq(1:100), seq(1:100), type = 'l', lwd = 3, lty = "solid", ylab = "Cumulative Percentage of Reviews", xlab = "Percentage of Reviewers")
plot_line_percentage_percentage(percentage_percentage_freq(openA$Rust), lty = "solid")
plot_line_percentage_percentage(percentage_percentage_freq(openOwnRec$Rust), lty = "dotted", pch = 2)
plot_line_percentage_percentage(percentage_percentage_freq(openSV2$Rust), lty = "twodash", pch = 6)
plot_line_percentage_percentage(percentage_percentage_freq(openWhoDo$Rust), lty = "dashed", pch = 6)
#plot_line_percentage_percentage(percentage_percentage_freq(openLearnRec$Rust), lty = "dashed", pch = 6)
abline(v = 1.5)
abline(v = 10)
abline(v = 20)
abline(h = 80)
abline(h = 60)
abline(h = 20)
legend(50, 20, legend=c("RevOwnRec", "Actual Workload", "SofiaWL", "WhoDo", "x = y"), lty=c('dotted', 'solid', 'twodash', 'dashed', 'solid'), lwd = 3, cex=1.2)
dev.off()

#legend(50, 20, legend=c("RevOwnRec", "Actual Workload", "LearnRec", "x = y"), lty=c('dotted', 'solid', 'dashed', 'solid'), lwd = 3, cex=1.2)

#skewness
percentage_difference(skewness(openA$Kubernetes, na.rm = TRUE), skewness(openAuthRec$Kubernetes, na.rm = TRUE))
percentage_difference(skewness(openA$Kubernetes, na.rm = TRUE), skewness(openOwnRec$Kubernetes, na.rm = TRUE))
percentage_difference(skewness(openA$Kubernetes, na.rm = TRUE), skewness(opencH$Kubernetes, na.rm = TRUE))
percentage_difference(skewness(openA$Kubernetes, na.rm = TRUE), skewness(openLearnRec$Kubernetes, na.rm = TRUE))
percentage_difference(skewness(openA$Kubernetes, na.rm = TRUE), skewness(openRet$Kubernetes, na.rm = TRUE))
percentage_difference(skewness(openA$Kubernetes, na.rm = TRUE), skewness(openTurnRec$Kubernetes, na.rm = TRUE))
percentage_difference(skewness(openA$Kubernetes, na.rm = TRUE), skewness(openSV1$Kubernetes, na.rm = TRUE))
percentage_difference(skewness(openA$Kubernetes, na.rm = TRUE), skewness(openWhoDo$Kubernetes, na.rm = TRUE))
percentage_difference(skewness(openA$Kubernetes, na.rm = TRUE), skewness(openSV2$Kubernetes, na.rm = TRUE))

