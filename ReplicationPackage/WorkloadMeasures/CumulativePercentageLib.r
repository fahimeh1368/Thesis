#to calculate AUC
install.packages("MESS")
library(MESS)

percentage_difference <- function(a, s) {
  ((s/a)-1) *100
}

percentage_percentage_freq <- function(x, decreasing = TRUE) {
  x <- na.omit(x)
  x <- sort(x, decreasing = decreasing)
  percentage = seq(1:length(x))/
        length(x)*100
  cumulative = cumsum(x/sum(x)*100)
  data.frame(cumulative, percentage)
}


get_y_cumulative_at_percentage <- function(x, percentage, decreasing = TRUE) {
  x <- na.omit(x)
  x_at_y = sum(x)*(percentage/100)
  df = percentage_percentage_freq(x, decreasing)
  length(df[df$cumulative <= x_at_y ,])/length(x);
  
}
 
get_cumulative_at_df <- function(df, percentage) {
  r = df$cumulative[length(df$cumulative)*(percentage/100)]
  if (min(percentage) == 0) {
    r = append(0,r)
  }
  r
}

get_cumulative_at_raw <- function(x, percentage) {
  na.omit(x)
  df <- percentage_percentage_freq(x)
  get_cumulative_at_df(df, percentage)
}

plot_percentage_percentage <- function(ppf) {
  plot(
    ppf$percentage, ppf$cumulative, 
    type = 'l', lwd = 3, ylab = ""
  )
  #ppf
}

plot_line_percentage_percentage <- function(ppf, lty = 1, col = "black", pch = 2, lwd = 3) {
  lines(
    ppf$percentage, ppf$cumulative,
    type = 'l', lwd = lwd,
    lty = lty, col = col, pch = pch 
  )
  #ppf
}

plot_percentage_freqency <- function(x) {
  x <- na.omit(x)
  x = sort(x, decreasing = T)
  plot(cumsum(x/sum(x)*100), type = 'l')
  
}

plot_line_percentage_freqency <- function(x, lty) {
  x <- na.omit(x)
  lines(cumsum(x/sum(x)*100), type = 'l', lty = lty)
}

auc_difference_percentage <- function(a, s) {
  
  da <- percentage_percentage_freq(a);
  ds <- percentage_percentage_freq(s);
  
  #total area - doesn't makes sense as below even distribution is impossible given ranking and summing
  #percentage_difference(auc(da$percentage, da$cumulative, from = 1, to = 100), auc(ds$percentage, ds$cumulative, from = 1, to = 100))
  
  #relative getting rid of the area that is impossible to achieve based on cumulative distribution and rank (is a weighted avg)
  percentage_difference(auc(da$percentage, da$cumulative)-5000, auc(ds$percentage, ds$cumulative)-5000)
  
}

# get the cumulative proportion of workload at p
cumulative_at_p_all <- function(d, p) {
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


# calculate the workload at a given quantile
quantile_all <- function(d, p) {
  r <- c(quantile(na.omit(d$CoreFX), p, names = F), 
         quantile(na.omit(d$CoreCLR), p, names = F), 
         quantile(na.omit(d$Roslyn), p, names = F), 
         quantile(na.omit(d$Rust), p, names = F), 
         quantile(na.omit(d$Kubernetes), p, names = F)
  )
  print(mean(r))
  r
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
auc_difference_percentage_Simulated <- function(a, s) {
  r<-{}
 
  for (i in 1:length(s)){
  r[i] <- c(auc_difference_percentage(a[col(s)==i], s[col(s)==i]))
}
  print(mean(r))
  
}

