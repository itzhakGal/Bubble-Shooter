
import numpy as np
import pandas as pd
import qpsolvers as qps

import warnings
warnings.simplefilter("ignore",category=UserWarning)
import matplotlib.pyplot as plt
import matplotlib
from sklearn.model_selection import train_test_split



#targil 1

def getXy(nameFile):
   df = pd.read_csv(nameFile)
   X = np.array(df[['x1', 'x2']])
   y = np.array(df["y"].values)
   y = np.where(y == 0, -1, 1)
   return X,y

def plot(X,y,w):

   x_min = np.amin(X[:, 0])
   x_max = np.amax(X[:, 0])
   y_min = np.amin(X[:, 1])
   y_max = np.amax(X[:, 1])
   r = np.where(y < 0)
   b = np.where(y > 0)
   plt.plot(X[r, 0], X[r, 1], 'o', color='red')
   plt.plot(X[b, 0], X[b, 1], 'o', color='blue')
   plt.axis([x_min - 1, x_max + 1, y_min - 1, y_max + 1])

   lx = np.linspace(x_min, x_max, 60)

   ly = [(-w[-1] - w[0] * p) / w[1] for p in lx]
   plt.plot(lx, ly, color='black')

   ly1 = [(-w[-1] - w[0] * p - 1) / w[1] for p in lx]
   plt.plot(lx, ly1, "--", color='red')

   ly2 = [(-w[-1] - w[0] * p + 1) / w[1] for p in lx]
   plt.plot(lx, ly2, "--", color='blue')
   return plt

def plotSupportVectors(plt,alpha,thresh,X):
    support_vectors = np.argwhere(np.abs(alpha) > thresh).reshape(-1)
    plt.scatter(X[support_vectors, 0], X[support_vectors, 1], s=150, linewidth=2, facecolors='none', edgecolors='k')
def mainQ11():
   print("The quadratic scheme of SVM:")
   X,y=getXy('simple_classification.csv')
   X = np.c_[X, np.ones(X.shape[0])]
   P = np.eye(3)
   q = np.zeros(3)
   G = -np.diag(y) @ X
   h = -np.ones(X.shape[0])

   w = qps.solve_qp(P, q, G, h, solver='osqp')

   print("w = {}".format(w))
   plt=plot(X,y,w)
   plt.show()


def mainQ12():
   print("The dual scheme of SVM:")
   X, y = getXy('simple_classification.csv')
   N = X.shape[0]
   X = np.c_[X, np.ones(N)]
   G = np.diag(y) @ X
   P = G @ G.T
   q = -np.ones(N)
   GG = -np.eye(N)
   h = np.zeros(N)
   alpha = qps.solve_qp(P, q, GG, h, solver='osqp')
   w = G.T @ alpha
   print("w = {}".format(w))
   plt = plot(X, y, w)
   plotSupportVectors(plt,alpha,0.1,X)
   plt.show()



##targil 22

import itertools


def plot_data(X, y, zoom_out=False, s=None):
   if zoom_out:
      x_min = np.amin(X[:, 0])
      x_max = np.amax(X[:, 0])
      y_min = np.amin(X[:, 1])
      y_max = np.amax(X[:, 1])

      plt.axis([x_min - 1, x_max + 1, y_min - 1, y_max + 1])

   plt.scatter(X[:, 0], X[:, 1], c=y, s=s, cmap=matplotlib.colors.ListedColormap(['blue', 'red']))

def svm_dual_kernel(X, y, ker, max_iter=4000, verbose=False):
   N = X.shape[0]
   P = np.empty((N, N))
   for i, j in itertools.product(range(N), range(N)):
      P[i, j] = y[i] * y[j] * ker(X[i, :], X[j, :])
   P = 0.5 * (P + P.T)
   P = 0.5 * P
   q = -np.ones(N)
   GG = -np.eye(N)
   h = np.zeros(N)

   alpha = qps.solve_qp(P, q, GG, h, solver='osqp', max_iter=max_iter, verbose=verbose)
   # w = \sum_i alpha_iy_ix_i
   # w = G.T @ alpha

   return alpha

def plot_classifier_z_kernel(alpha, X, y, ker, s=None):
   x_min = np.amin(X[:, 0])
   x_max = np.amax(X[:, 0])
   y_min = np.amin(X[:, 1])
   y_max = np.amax(X[:, 1])

   xx = np.linspace(x_min, x_max)
   yy = np.linspace(y_min, y_max)

   xx, yy = np.meshgrid(xx, yy)

   N = X.shape[0]
   z = np.zeros(xx.shape)
   for i, j in itertools.product(range(xx.shape[0]), range(xx.shape[1])):
      z[i, j] = sum([y[k] * alpha[k] * ker(X[k, :], np.array([xx[i, j], yy[i, j]])) for k in range(N)])

   plt.rcParams["figure.figsize"] = [15, 10]

   plt.contour(xx, yy, z, levels=[-1, 0, 1], colors=['red', 'black', 'blue'], linestyles=['--', '-', '--'])

   plot_data(X, y, s=s)

def RBF_kernel(x, y):
   return np.e ** (-(x - y).T @ (x - y))

def poly_kernel(x, y,degree):
   return (1 + x.T @ y) ** degree

def sigmoid(X,y,k,c):
   return np.tanh(k*X.T*y+c)
def linearKernel(X,y):
   return X@y


def support_vectors(alpha, thresh=0.3):
   return np.argwhere(np.abs(alpha) > thresh).reshape(-1)


def highlight_support_vectors(X, alpha):
   sv = support_vectors(alpha)
   plt.scatter(X[sv, 0], X[sv, 1], s=300, linewidth=3, facecolors='none', edgecolors='k')
def mainQ2():
   X,y=getXy('simple_nonlin_classification.csv')
   plot_data(X, y)
   rbf(X,y)


def rbf(X,y):

   alpha = svm_dual_kernel(X, y, RBF_kernel, max_iter=1000000, verbose=False)
   plot_classifier_z_kernel(alpha, X, y, RBF_kernel, s=80)
   highlight_support_vectors(X, alpha)


def main_Q2():
   df = pd.read_csv('simple_nonlin_classification.csv')
   X = np.array(df[['x1', 'x2']])
   y = np.array(df['y'])
   X_train, X_test, y_train, y_test = train_test_split(X, y, test_size=0.2, random_state=42)
   degrees = [2, 3, 4]
   gammas = [0.1, 0.5, 1]
   score_poly = []
   score_rbf = []

   for degree in degrees:
      alpha_poly = kernel_dual_SVM(X_train, y_train, 'poly', degree=degree)
      poly_predicted_correctly = 0
      for i in range(len(y_test)):
         if y_test[i] == predict(alpha_poly, y_train, X_train, X_test[i], 'poly', degree=degree):
            poly_predicted_correctly += 1
      accuracy = poly_predicted_correctly / len(y_test)
      print('The score for polynomial kernel with degree', degree, 'is - ', accuracy)
      score_poly.append(accuracy)

   for gamma in gammas:
      alpha_rbf = kernel_dual_SVM(X_train, y_train, 'rbf', gamma=gamma)
      rbf_predicted_correctly = 0
      for i in range(len(y_test)):
         if y_test[i] == predict(alpha_rbf, y_train, X_train, X_test[i], 'poly', degree=degree):
            rbf_predicted_correctly += 1
      accuracy = rbf_predicted_correctly / len(y_test)
      print('The score for rbf kernel with gamma ', gamma, 'is - ', accuracy)
      score_rbf.append(accuracy)

   best_poly_index = np.argmax(score_poly)
   best_rbf_index = np.argmax(score_rbf)
   best_poly_degree = degrees[best_poly_index]
   best_rbf_gamma = gammas[best_rbf_index]

   alpha_poly = kernel_dual_SVM(X_train, y_train, 'poly', degree=best_poly_degree)
   alpha_rbf = kernel_dual_SVM(X_train, y_train, 'rbf', gamma=best_rbf_gamma)

   plot_kernel_SVM(X_train, y_train, alpha_poly, 'poly', degree=best_poly_degree, thresh=30)
   plot_kernel_SVM(X_train, y_train, alpha_rbf, 'rbf', gamma=best_rbf_gamma, thresh=200)


def kernel(x, y, kernel_type, degree=None, gamma=None):
   if kernel_type == 'poly':
      return (1 + np.dot(y, x)) ** degree
   elif kernel_type == 'rbf':
      return np.e ** (-gamma * ((x - y).T @ (x - y)))


def predict(alpha, y, X_train, X_test, kernel_type, degree=None, gamma=None):
   ker = lambda x1, x2: kernel(x1, x2, kernel_type, degree, gamma)
   prediction = 0
   for i in range(X_train.shape[0]):
      prediction += alpha[i] * y[i] * ker(X_train[i], X_test)
   return np.sign(prediction)


def kernel_dual_SVM(X, y, kernel_type, degree=None, gamma=None, max_iter=4000, verbose=False):
   N = X.shape[0]
   P = np.empty((N, N))
   for i, j in itertools.product(range(N), range(N)):
      P[i, j] = y[i] * y[j] * kernel(X[i, :], X[j, :], kernel_type, degree, gamma)
   P = 0.5 * (P + P.T)
   P = 0.5 * P
   q = -np.ones(N)
   GG = -np.eye(N)
   h = np.zeros(N)
   alpha = qps.solve_qp(P, q, GG, h, solver='osqp', max_iter=max_iter, verbose=verbose)
   return alpha


def plot_kernel_SVM(X, y, alpha, kernel_type, degree=None, gamma=None, s=None, thresh=0.9):
   ker = lambda x1, x2: kernel(x1, x2, kernel_type, degree, gamma)

   x_max = np.amax(X[:, 0])
   x_min = np.amin(X[:, 0])
   y_min = np.amin(X[:, 1])
   y_max = np.amax(X[:, 1])

   xx = np.linspace(x_min, x_max, 100)
   yy = np.linspace(y_min, y_max, 100)
   xx, yy = np.meshgrid(xx, yy)

   N = X.shape[0]
   z = np.zeros(xx.shape)

   for i, j in itertools.product(range(xx.shape[0]), range(xx.shape[1])):
      z[i, j] = sum([y[k] * alpha[k] * ker(X[k, :], np.array([xx[i, j], yy[i, j]])) for k in range(N)])

   plt.rcParams["figure.figsize"] = [15, 10]
   plt.contour(xx, yy, z, levels=[-1, 0, 1], colors=['cornflowerblue', 'slategray', 'lightskyblue'],
               linestyles=['--', '-', '--'])
   plot_data(X, y, s=s)
   support_vectors = np.argwhere(np.abs(alpha) > thresh).reshape(-1)
   plt.scatter(X[support_vectors, 0], X[support_vectors, 1], s=130, linewidth=1.5, facecolors='none',
               edgecolors='slategray')
   plt.xlabel('X1')
   plt.ylabel('X2')
   title = kernel_type + 'kernel SVM Classification'
   plt.title(title)
   plt.show()



if __name__ == '__main__':
   mainQ11()
