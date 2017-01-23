u s i n g   S y s t e m ;  
 u s i n g   U n i t y E n g i n e ;  
 u s i n g   U n i t y S t a n d a r d A s s e t s . C r o s s P l a t f o r m I n p u t ;  
 n a m e s p a c e   U n i t y S t a n d a r d A s s e t s . _ 2 D  
 {  
         p u b l i c   c l a s s   P l a t f o r m e r C h a r a c t e r 2 D   :   M o n o B e h a v i o u r  
         {  
                 [ S e r i a l i z e F i e l d ]   p r i v a t e   f l o a t   m _ M a x S p e e d   =   1 0 f ;                                         / /   T h e   f a s t e s t   t h e   p l a y e r   c a n   t r a v e l   i n   t h e   x   a x i s .  
                 [ S e r i a l i z e F i e l d ]   p r i v a t e   f l o a t   m _ J u m p F o r c e   =   1 0 f ;                                     / /   A m o u n t   o f   f o r c e   a d d e d   w h e n   t h e   p l a y e r   j u m p s .  
                 [ R a n g e ( 0 ,   1 ) ]   [ S e r i a l i z e F i e l d ]   p r i v a t e   f l o a t   m _ C r o u c h S p e e d   =   . 3 6 f ;     / /   A m o u n t   o f   m a x S p e e d   a p p l i e d   t o   c r o u c h i n g   m o v e m e n t .   1   =   1 0 0 %  
                 [ S e r i a l i z e F i e l d ]   p r i v a t e   b o o l   m _ A i r C o n t r o l   =   f a l s e ;                                   / /   W h e t h e r   o r   n o t   a   p l a y e r   c a n   s t e e r   w h i l e   j u m p i n g ;  
                 [ S e r i a l i z e F i e l d ]   p r i v a t e   L a y e r M a s k   m _ W h a t I s G r o u n d ;                                     / /   A   m a s k   d e t e r m i n i n g   w h a t   i s   g r o u n d   t o   t h e   c h a r a c t e r  
 	 	 [ S e r i a l i z e F i e l d ]   p r i v a t e   V e c t o r 2   G r o u n d N o r m a l ;  
 	 	 [ S e r i a l i z e F i e l d ]   p r i v a t e   b o o l   m _ G r o u n d e d ;          
 	 	 [ S e r i a l i z e F i e l d ]   p r i v a t e   L a y e r M a s k   m a s k ;  
  
                 p r i v a t e   T r a n s f o r m   m _ G r o u n d C h e c k ;         / /   A   p o s i t i o n   m a r k i n g   w h e r e   t o   c h e c k   i f   t h e   p l a y e r   i s   g r o u n d e d .  
                 c o n s t   f l o a t   k _ G r o u n d e d R a d i u s   =   . 2 f ;   / /   R a d i u s   o f   t h e   o v e r l a p   c i r c l e   t o   d e t e r m i n e   i f   g r o u n d e d  
                 p r i v a t e   T r a n s f o r m   m _ C e i l i n g C h e c k ;       / /   A   p o s i t i o n   m a r k i n g   w h e r e   t o   c h e c k   f o r   c e i l i n g s  
                 c o n s t   f l o a t   k _ C e i l i n g R a d i u s   =   . 0 1 f ;   / /   R a d i u s   o f   t h e   o v e r l a p   c i r c l e   t o   d e t e r m i n e   i f   t h e   p l a y e r   c a n   s t a n d   u p  
                 p r i v a t e   A n i m a t o r   m _ A n i m ;                         / /   R e f e r e n c e   t o   t h e   p l a y e r ' s   a n i m a t o r   c o m p o n e n t .  
                 p r i v a t e   R i g i d b o d y 2 D   m _ R i g i d b o d y 2 D ;  
 	 	 p r i v a t e   S p r i t e R e n d e r e r   m _ S p r i t e R e n d e r e r ;  
 	 	 p r i v a t e   T r a n s f o r m   m _ L e f t F o o t ;  
 	 	 p r i v a t e   T r a n s f o r m   m _ R i g h t F o o t ;  
 	 	 p r i v a t e   T r a n s f o r m   m _ M i d F o o t ;  
 	 	 p r i v a t e   L i n e R e n d e r e r   m _ D e b u g L i n e ;  
 	 	 p r i v a t e   L i n e R e n d e r e r   m _ L e f t L i n e ;  
 	 	 p r i v a t e   L i n e R e n d e r e r   m _ M i d L i n e ;  
 	 	 p r i v a t e   L i n e R e n d e r e r   m _ R i g h t L i n e ;  
 	 	 p r i v a t e   V e c t o r 2   P V e l ;  
 	 	 p r i v a t e   b o o l   l e f t c o n t a c t ;  
 	 	 p r i v a t e   b o o l   m i d c o n t a c t ;  
 	 	 p r i v a t e   b o o l   r i g h t c o n t a c t ;  
 	 	 p r i v a t e   b o o l   m _ J u m p ;  
  
  
  
                 p r i v a t e   v o i d   A w a k e ( )  
                 {  
                         / /   S e t t i n g   u p   r e f e r e n c e s .  
                         m _ G r o u n d C h e c k   =   t r a n s f o r m . F i n d ( " G r o u n d C h e c k " ) ;  
 	 	 	 m _ L e f t F o o t   =   t r a n s f o r m . F i n d ( " L e f t F o o t " ) ;  
 	 	 	 m _ L e f t L i n e   =   m _ L e f t F o o t . G e t C o m p o n e n t < L i n e R e n d e r e r > ( ) ;  
  
 	 	 	 m _ M i d F o o t   =   t r a n s f o r m . F i n d ( " M i d F o o t " ) ;  
 	 	 	 m _ M i d L i n e   =   m _ M i d F o o t . G e t C o m p o n e n t < L i n e R e n d e r e r > ( ) ;  
  
 	 	 	 m _ D e b u g L i n e   =   G e t C o m p o n e n t < L i n e R e n d e r e r > ( ) ;  
 	 	 	 m _ R i g h t F o o t   =   t r a n s f o r m . F i n d ( " R i g h t F o o t " ) ;  
 	 	 	 m _ R i g h t L i n e   =   m _ R i g h t F o o t . G e t C o m p o n e n t < L i n e R e n d e r e r > ( ) ;  
                         m _ C e i l i n g C h e c k   =   t r a n s f o r m . F i n d ( " C e i l i n g C h e c k " ) ;  
                         m _ A n i m   =   G e t C o m p o n e n t < A n i m a t o r > ( ) ;  
                         m _ R i g i d b o d y 2 D   =   G e t C o m p o n e n t < R i g i d b o d y 2 D > ( ) ;  
 	 	 	 m _ S p r i t e R e n d e r e r   =   G e t C o m p o n e n t < S p r i t e R e n d e r e r > ( ) ;  
                 }  
  
  
                 p r i v a t e   v o i d   F i x e d U p d a t e ( )  
 	 	 {  
 	 	 	 f l o a t   h   =   C r o s s P l a t f o r m I n p u t M a n a g e r . G e t A x i s ( " H o r i z o n t a l " ) ;  
 	 	 	 m _ G r o u n d e d   =   f a l s e ;  
 	 	 	 m _ S p r i t e R e n d e r e r . c o l o r   =   C o l o r . w h i t e ;  
 	 	 	 m _ L e f t L i n e . S e t C o l o r s   ( C o l o r . r e d ,   C o l o r . r e d ) ;  
 	 	 	 m _ R i g h t L i n e . S e t C o l o r s   ( C o l o r . r e d ,   C o l o r . r e d ) ;  
  
 	 	 	 / *  
 	 	 	 R a y c a s t H i t 2 D   L e f t H i t   =   P h y s i c s 2 D . R a y c a s t   ( m _ L e f t F o o t . p o s i t i o n ,   V e c t o r 2 . d o w n ,   0 . 7 5 f ,   m a s k ) ;  
 	 	 	 i f   ( L e f t H i t . c o l l i d e r   ! =   n u l l )  
 	 	 	 {  
 	 	 	 	 l e f t c o n t a c t   =   t r u e ;  
 	 	 	 	 m _ L e f t L i n e . S e t C o l o r s   ( C o l o r . g r e e n ,   C o l o r . g r e e n ) ;  
 	 	 	 }    
 	 	 	 e l s e  
 	 	 	 {  
 	 	 	 	 l e f t c o n t a c t   =   f a l s e ;  
 	 	 	 }  
 	 	 	 * /  
  
 	 	 	 R a y c a s t H i t 2 D   M i d H i t   =   P h y s i c s 2 D . R a y c a s t   ( m _ M i d F o o t . p o s i t i o n ,   V e c t o r 2 . d o w n ,   0 . 7 5 f ,   m a s k ) ;  
 	 	 	 i f   ( M i d H i t . c o l l i d e r   ! =   n u l l )    
 	 	 	 {  
 	 	 	 	 m i d c o n t a c t   =   t r u e ;  
 	 	 	 	 m _ M i d L i n e . S e t C o l o r s   ( C o l o r . g r e e n ,   C o l o r . g r e e n ) ;  
 	 	 	 }    
 	 	 	 e l s e    
 	 	 	 {  
 	 	 	 	 m i d c o n t a c t   =   f a l s e ;  
 	 	 	 }  
  
 	 	 	 i f   ( m i d c o n t a c t )    
 	 	 	 {  
 	 	 	 	 m _ G r o u n d e d   =   t r u e ;  
 	 	 	 }    
 	 	 	 e l s e    
 	 	 	 {  
 	 	 	 	 m _ G r o u n d e d   =   f a l s e ;  
 	 	 	 }  
  
 	 	 	 / *  
 	 	 	 R a y c a s t H i t 2 D   R i g h t H i t   =   P h y s i c s 2 D . R a y c a s t   ( m _ R i g h t F o o t . p o s i t i o n ,   V e c t o r 2 . d o w n ,   0 . 7 5 f ,   m a s k ) ;  
 	 	 	 i f   ( R i g h t H i t . c o l l i d e r   ! =   n u l l )    
 	 	 	 {  
 	 	 	 	 r i g h t c o n t a c t   =   t r u e ;  
 	 	 	 	 m _ R i g h t L i n e . S e t C o l o r s   ( C o l o r . g r e e n ,   C o l o r . g r e e n ) ;  
 	 	 	 }    
 	 	 	 e l s e    
 	 	 	 {  
 	 	 	 	 r i g h t c o n t a c t   =   f a l s e ;  
 	 	 	 }  
  
  
  
 	 	 	 i f   ( r i g h t c o n t a c t   & &   l e f t c o n t a c t )    
 	 	 	 {  
 	 	 	 	 m _ G r o u n d e d   =   t r u e ;  
 	 	 	 }    
 	 	 	 e l s e    
 	 	 	 {  
 	 	 	 	 m _ G r o u n d e d   =   f a l s e ;  
 	 	 	 }  
 	 	 	 * /  
  
  
 	 	 	 i f   ( m _ G r o u n d e d )   / / R i g h t H i t . d i s t a n c e   < =   0 . 1 f   | |   L e f t H i t . d i s t a n c e   < =   0 . 1 f  
 	 	 	 {  
  
 	 	 	 	 i f ( m _ R i g i d b o d y 2 D . v e l o c i t y . y   > =   0 )  
 	 	 	 	 {  
 	 	 	 	 	 i f   ( L e f t H i t . d i s t a n c e   > =   R i g h t H i t . d i s t a n c e )    
 	 	 	 	 	 {  
 	 	 	 	 	 	 G r o u n d N o r m a l   =   L e f t H i t . n o r m a l ;  
 	 	 	 	 	 	 / / p r i n t ( L e f t H i t . n o r m a l ) ;  
 	 	 	 	 	 	 / / m _ R i g i d b o d y 2 D . p o s i t i o n   =   n e w   V e c t o r 2   ( m _ R i g i d b o d y 2 D . p o s i t i o n . x ,   m _ R i g i d b o d y 2 D . p o s i t i o n . y + ( 0 . 5 f   -   L e f t H i t . d i s t a n c e ) ) ;  
 	 	 	 	 	 }    
 	 	 	 	 	 e l s e    
 	 	 	 	 	 {  
 	 	 	 	 	 	 G r o u n d N o r m a l   =   R i g h t H i t . n o r m a l ;  
 	 	 	 	 	 	 / / p r i n t ( R i g h t H i t . n o r m a l ) ;  
 	 	 	 	 	 	 / / m _ R i g i d b o d y 2 D . p o s i t i o n   =   n e w   V e c t o r 2   ( m _ R i g i d b o d y 2 D . p o s i t i o n . x ,   m _ R i g i d b o d y 2 D . p o s i t i o n . y + ( 0 . 5 f   -   R i g h t H i t . d i s t a n c e ) ) ;  
 	 	 	 	 	 }  
 	 	 	 	 }  
 	 	 	 	 e l s e  
 	 	 	 	 {  
 	 	 	 	 	 i f   ( L e f t H i t . d i s t a n c e   < =   R i g h t H i t . d i s t a n c e )    
 	 	 	 	 	 {  
 	 	 	 	 	 	 G r o u n d N o r m a l   =   L e f t H i t . n o r m a l ;  
 	 	 	 	 	 	 / / p r i n t ( L e f t H i t . n o r m a l ) ;  
 	 	 	 	 	 	 / / m _ R i g i d b o d y 2 D . p o s i t i o n   =   n e w   V e c t o r 2   ( m _ R i g i d b o d y 2 D . p o s i t i o n . x ,   m _ R i g i d b o d y 2 D . p o s i t i o n . y + ( 0 . 5 f   -   L e f t H i t . d i s t a n c e ) ) ;  
 	 	 	 	 	 }    
 	 	 	 	 	 e l s e    
 	 	 	 	 	 {  
 	 	 	 	 	 	 G r o u n d N o r m a l   =   R i g h t H i t . n o r m a l ;  
 	 	 	 	 	 	 / / p r i n t ( R i g h t H i t . n o r m a l ) ;  
 	 	 	 	 	 	 / / m _ R i g i d b o d y 2 D . p o s i t i o n   =   n e w   V e c t o r 2   ( m _ R i g i d b o d y 2 D . p o s i t i o n . x ,   m _ R i g i d b o d y 2 D . p o s i t i o n . y + ( 0 . 5 f   -   R i g h t H i t . d i s t a n c e ) ) ;  
 	 	 	 	 	 } 	  
 	 	 	 	 }  
 	 	 	 	 / *  
 	 	 	 	 i f ( R i g h t H i t . d i s t a n c e   < =   0 . 2 5 f   | |   L e f t H i t . d i s t a n c e   < =   0 . 2 5 f )  
 	 	 	 	 {  
 	 	 	 	 	 i f   ( L e f t H i t . d i s t a n c e   < =   R i g h t H i t . d i s t a n c e )    
 	 	 	 	 	 {  
 	 	 	 	 	 	 G r o u n d N o r m a l   =   L e f t H i t . n o r m a l ;  
 	 	 	 	 	 	 / / p r i n t ( L e f t H i t . n o r m a l ) ;  
 	 	 	 	 	 	 / / m _ R i g i d b o d y 2 D . p o s i t i o n   =   n e w   V e c t o r 2   ( m _ R i g i d b o d y 2 D . p o s i t i o n . x ,   m _ R i g i d b o d y 2 D . p o s i t i o n . y + ( 0 . 7 5 f   -   L e f t H i t . d i s t a n c e ) ) ;  
 	 	 	 	 	 }    
 	 	 	 	 	 e l s e    
 	 	 	 	 	 {  
 	 	 	 	 	 	 G r o u n d N o r m a l   =   R i g h t H i t . n o r m a l ;  
 	 	 	 	 	 	 / / p r i n t ( R i g h t H i t . n o r m a l ) ;  
 	 	 	 	 	 	 / / m _ R i g i d b o d y 2 D . p o s i t i o n   =   n e w   V e c t o r 2   ( m _ R i g i d b o d y 2 D . p o s i t i o n . x ,   m _ R i g i d b o d y 2 D . p o s i t i o n . y + ( 0 . 7 5 f   -   R i g h t H i t . d i s t a n c e ) ) ;  
 	 	 	 	 	 }  
 	 	 	 	 }  
 	 	 	 	 e l s e  
 	 	 	 	 {  
 	 	 	 	 	 i f   ( L e f t H i t . d i s t a n c e   > =   R i g h t H i t . d i s t a n c e )    
 	 	 	 	 	 {  
 	 	 	 	 	 	 G r o u n d N o r m a l   =   L e f t H i t . n o r m a l ;  
 	 	 	 	 	 	 / / p r i n t ( L e f t H i t . n o r m a l ) ;  
 	 	 	 	 	 	 / / m _ R i g i d b o d y 2 D . p o s i t i o n   =   n e w   V e c t o r 2   ( m _ R i g i d b o d y 2 D . p o s i t i o n . x ,   m _ R i g i d b o d y 2 D . p o s i t i o n . y + ( 0 . 5 f   -   L e f t H i t . d i s t a n c e ) ) ;  
 	 	 	 	 	 }    
 	 	 	 	 	 e l s e    
 	 	 	 	 	 {  
 	 	 	 	 	 	 G r o u n d N o r m a l   =   R i g h t H i t . n o r m a l ;  
 	 	 	 	 	 	 / / p r i n t ( R i g h t H i t . n o r m a l ) ;  
 	 	 	 	 	 	 / / m _ R i g i d b o d y 2 D . p o s i t i o n   =   n e w   V e c t o r 2   ( m _ R i g i d b o d y 2 D . p o s i t i o n . x ,   m _ R i g i d b o d y 2 D . p o s i t i o n . y + ( 0 . 5 f   -   R i g h t H i t . d i s t a n c e ) ) ;  
 	 	 	 	 	 }  
 	 	 	 	 }  
 	 	 	 	 * /  
  
 	 	 	  
 	 	 	 	 G r o u n d N o r m a l   =   M i d H i t . n o r m a l ;  
 	 	 	 }  
  
 	 	 	 i f   ( ! m _ G r o u n d e d )    
 	 	 	 {  
 	 	 	 	 m _ R i g i d b o d y 2 D . v e l o c i t y   =   n e w   V e c t o r 2 ( m _ R i g i d b o d y 2 D . v e l o c i t y . x ,   m _ R i g i d b o d y 2 D . v e l o c i t y . y   -   1 ) ;  
 	 	 	 	 / / P V e l . y   - =   1 ;  
 	 	 	 }   e l s e   {  
 	 	 	 	 / / P V e l . y   =   0 ;  
 	 	 	 }  
 	 	 	 	  
 	 	 	 i f   ( m _ G r o u n d e d )    
 	 	 	 {  
 	 	 	 	 V e c t o r 2   g r o u n d p e r p ;  
 	 	 	 	 V e c t o r 2   A d j u s t e d V e l ;  
  
 	 	 	 / / 	 i f ( m _ F a c i n g R i g h t )  
 	 	 	 / / 	 {  
 	 	 	 	 	 g r o u n d p e r p . x   =   G r o u n d N o r m a l . y ;  
 	 	 	 	 	 g r o u n d p e r p . y   =   - G r o u n d N o r m a l . x ;  
 	 	 	 / / 	 }  
 	 	 	 / / 	 e l s e  
 	 	 	 / / 	 {  
 	 	 	 / / 	 	 g r o u n d p e r p . x   =   - G r o u n d N o r m a l . y ;  
 	 	 	 / / 	 	 g r o u n d p e r p . y   =   G r o u n d N o r m a l . x ;  
 	 	 	 / / 	 }  
  
 	 	 	 	 / / m _ R i g i d b o d y 2 D . v e l o c i t y . x   + =   h ;  
 	 	 	 	 / / m _ R i g i d b o d y 2 D . v e l o c i t y   =   n e w   V e c t o r 2 (   m _ R i g i d b o d y 2 D . v e l o c i t y . x   +   h * 1 0   , m _ R i g i d b o d y 2 D . v e l o c i t y . y ) ;  
  
 	 	 	 	 / / g r o u n d p e r p . x   =   G r o u n d N o r m a l . y ;  
 	 	 	 	 / / g r o u n d p e r p . y   =   - G r o u n d N o r m a l . x ;  
  
 	 	 	 	 / / f l o a t   t e s t   =   V e c t o r 2 . D o t ( m _ R i g i d b o d y 2 D . v e l o c i t y ,   g r o u n d p e r p ) ;  
 	 	 	 	 / / p r i n t ( t e s t ) ;  
 	 	 	 	 f l o a t   p r o j e c t i o n V a l   =   V e c t o r 2 . D o t ( m _ R i g i d b o d y 2 D . v e l o c i t y ,   g r o u n d p e r p ) / g r o u n d p e r p . s q r M a g n i t u d e ;  
 	 	 	 	 / / p r i n t ( p r o j e c t i o n V a l ) ;  
 	 	 	 	 A d j u s t e d V e l   =   g r o u n d p e r p   *   p r o j e c t i o n V a l ;  
 	 	 	 	 / / p r i n t ( A d j u s t e d V e l ) ;  
  
 	 	 	 	 i f ( A d j u s t e d V e l . n o r m a l i z e d . x   <   0 )  
 	 	 	 	 {  
 	 	 	 	 	 h   =   - h ;  
 	 	 	 	 }  
  
 	 	 	 	 i f ( m _ R i g i d b o d y 2 D . v e l o c i t y   = =   V e c t o r 2 . z e r o )  
 	 	 	 	 {  
 	 	 	 	 	 m _ R i g i d b o d y 2 D . v e l o c i t y   =   n e w   V e c t o r 2 ( h ,   m _ R i g i d b o d y 2 D . v e l o c i t y . y ) ;  
 	 	 	 	 	 p r i n t ( " F U C K Y O U " ) ;  
 	 	 	 	 }  
 	 	 	 	 e l s e  
 	 	 	 	 {  
 	 	 	 	 	 m _ R i g i d b o d y 2 D . v e l o c i t y   =   A d j u s t e d V e l   +   A d j u s t e d V e l . n o r m a l i z e d * h ;  
 	 	 	 	 }  
  
 	 	 	 	 i f ( m _ J u m p )  
 	 	 	 	 {  
 	 	 	 	 	 m _ R i g i d b o d y 2 D . v e l o c i t y   =   n e w   V e c t o r 2 ( m _ R i g i d b o d y 2 D . v e l o c i t y . x ,   m _ R i g i d b o d y 2 D . v e l o c i t y . y   +   m _ J u m p F o r c e ) ;  
 	 	 	 	 	 m _ J u m p   =   f a l s e ;  
 	 	 	 	 }  
 	 	 	 	 	  
  
 	 	 	 	 / / m _ R i g i d b o d y 2 D . v e l o c i t y   =   n e w   V e c t o r 2   ( m _ R i g i d b o d y 2 D . v e l o c i t y . x ,   P V e l . y ) ;   / / r e m o v e   l a t e r  
 	 	 	 }  
  
  
 	 	 	 p r i n t ( G r o u n d N o r m a l ) ;  
  
 	 	 	 m _ D e b u g L i n e . S e t P o s i t i o n ( 1 ,   m _ R i g i d b o d y 2 D . v e l o c i t y ) ;  
 	 	 	 / / m _ D e b u g L i n e . S e t P o s i t i o n ( 1 ,   G r o u n d N o r m a l ) ;  
  
  
                 }  
  
 	 	 p r i v a t e   v o i d   U p d a t e ( )  
 	 	 {  
 	 	 	 i f   ( ! m _ J u m p   & &   m _ G r o u n d e d )  
 	 	 	 {  
 	 	 	 	 / /   R e a d   t h e   j u m p   i n p u t   i n   U p d a t e   s o   b u t t o n   p r e s s e s   a r e n ' t   m i s s e d .  
 	 	 	 	 m _ J u m p   =   C r o s s P l a t f o r m I n p u t M a n a g e r . G e t B u t t o n D o w n ( " J u m p " ) ;  
 	 	 	 }  
 	 	 }  
         }  
 } 