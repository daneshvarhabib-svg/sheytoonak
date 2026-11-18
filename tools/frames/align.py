"""Warp picture B onto picture A (same scene, regenerated at another size): scale + shift from ORB matches.
align(A_rgb, B_rgb) -> (B warped to A's frame on green, inliers)"""
import cv2, numpy as np
def align(A, B):
    ga=cv2.cvtColor(A,cv2.COLOR_RGB2GRAY); gb=cv2.cvtColor(B,cv2.COLOR_RGB2GRAY)
    orb=cv2.ORB_create(6000); ka,da=orb.detectAndCompute(ga,None); kb,db=orb.detectAndCompute(gb,None)
    m=cv2.BFMatcher(cv2.NORM_HAMMING,crossCheck=True).match(db,da)
    src=np.float32([kb[x.queryIdx].pt for x in m]); dst=np.float32([ka[x.trainIdx].pt for x in m])
    M,inl=cv2.estimateAffinePartial2D(src,dst,method=cv2.RANSAC,ransacReprojThreshold=4)
    n=int(inl.sum()) if inl is not None else 0
    s=float(np.hypot(M[0,0],M[1,0])); M=np.float32([[s,0,M[0,2]],[0,s,M[1,2]]])   # no rotation
    W=cv2.warpAffine(B,M,(A.shape[1],A.shape[0]),flags=cv2.INTER_AREA if s<1 else cv2.INTER_LANCZOS4,borderValue=(40,240,30))
    return W,n,s
