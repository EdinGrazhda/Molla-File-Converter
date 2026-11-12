import {AbsoluteFill, CanvasImage, Composition, Easing, Interactive, interpolate, staticFile, useCurrentFrame, useVideoConfig} from "remotion";

const clamp = {extrapolateLeft: "clamp", extrapolateRight: "clamp"} as const;

const FileCard = ({output = false}: {output?: boolean}) => (
  <div style={{width:152, padding:"20px 12px 16px", boxSizing:"border-box", borderRadius:24,
    background:output ? "#e4f5e9" : "#f9fbfc", border:"2px solid #ffffff", boxShadow:"0 14px 35px #00182426", color:"#193e4b", textAlign:"center"}}>
    <svg width="64" height="65" viewBox="0 0 64 65">
      <path d="M12 3H40L54 17V61H12Z" fill={output ? "#a7d4b4" : "#bad8e0"} />
      <path d="M40 3V17H54" fill="none" stroke="#fff" strokeWidth="3"/>
      <path d="M20 47L30 34L36 41L43 33L49 47Z" fill="#fff"/>
      <circle cx="25" cy="25" r="4" fill="#fff"/>
    </svg>
    <div style={{fontSize:27, fontWeight:700, marginTop:4}}>{output ? "JPG" : "PNG"}</div>
    <div style={{fontSize:18, marginTop:8}}>{output ? "photo.jpg" : "photo.png"}</div>
  </div>
);

const MollaDemo = () => {
  const frame = useCurrentFrame();
  const {fps} = useVideoConfig();
  const t = frame / fps;
  return <AbsoluteFill style={{fontFamily:"MollaSans, sans-serif", color:"#193e4b",
    background:"radial-gradient(ellipse at 75% 40%, #d9eeee 0%, #eaf2f2 43%, #f5f1e9 100%)"}}>
    <style>{`@font-face {font-family:MollaSans;src:url("${staticFile("DejaVuSans.ttf")}");font-display:block;}`}</style>
    <Interactive.Div name="Brand" premountFor={fps} style={{position:"absolute", left:80, top:65, display:"flex", alignItems:"center", gap:18}}>
      <CanvasImage src={staticFile("molla-icon.png")} style={{width:62,height:62}} premountFor={fps}/>
      <span style={{fontSize:43,letterSpacing:-2}}>molla</span>
      <span style={{fontSize:22,marginLeft:20,color:"#59717c"}}>LOCAL FILE CONVERSION</span>
    </Interactive.Div>
    <Interactive.Div name="Headline" premountFor={fps} style={{position:"absolute",top:155,left:80,right:80,
      fontSize:76,letterSpacing:-4,lineHeight:1.1,opacity:interpolate(frame,[0,.7*fps],[0,1],clamp),
      translate:`0px ${interpolate(frame,[0,.7*fps],[20,0],clamp)}px`}}>
      Drop. Convert. Done.
    </Interactive.Div>
    <Interactive.Div name="UI demonstration" premountFor={fps} style={{position:"absolute",inset:0,
      opacity:interpolate(frame,[9*fps,9.6*fps],[1,0],clamp)}}>
      <div style={{position:"absolute",top:258,left:80,right:80,textAlign:"center",fontSize:32,color:"#58737c"}}>
        {t < 3.6 ? "Drop your image onto JPG." : t < 7 ? "A new format. The same original." : "Saved to Downloads / Molla."}
      </div>
      <CanvasImage src={staticFile("molla-ui.png")} premountFor={fps}
        style={{position:"absolute",left:280,top:310,width:520,height:592.8,filter:"drop-shadow(0px 20px 28px #25485524)"}}/>
      <Interactive.Div name="JPG drop target" premountFor={fps} style={{position:"absolute",left:623,top:568,width:98,height:66,borderRadius:24,
        boxShadow:"0 0 0 4px #729c8c, 0 0 28px #b9e9d5",background:"#bce7d330",
        opacity:interpolate(frame,[2.8*fps,3.2*fps,6.8*fps,7.1*fps],[0,1,1,0],clamp)}}/>
      <Interactive.Div name="Input file" premountFor={fps} style={{position:"absolute",
        left:interpolate(frame,[1.6*fps,3.5*fps],[60,596],{...clamp,easing:Easing.bezier(.4,0,.2,1)}),
        top:interpolate(frame,[1.6*fps,2.6*fps,3.5*fps],[485,420,518],{...clamp,easing:Easing.bezier(.4,0,.2,1)}),
        scale:interpolate(frame,[3.5*fps,3.9*fps],[1,.3],clamp),
        opacity:interpolate(frame,[.7*fps,1.2*fps,3.5*fps,3.9*fps],[0,1,1,0],clamp),
        rotate:`${interpolate(frame,[1.6*fps,2.4*fps,3.5*fps],[0,-6,0],clamp)}deg`}}>
        <FileCard/>
        <svg width="38" height="48" viewBox="0 0 38 48" style={{position:"absolute",left:115,top:105}}>
          <path d="M3 3L34 27L21 29L16 43L3 3Z" fill="#183e4b" stroke="white" strokeWidth="3"/>
        </svg>
      </Interactive.Div>
      <Interactive.Div name="Exported file" premountFor={fps} style={{position:"absolute",left:interpolate(frame,[7*fps,7.8*fps],[700,850],{...clamp,easing:Easing.bezier(.16,1,.3,1)}),
        top:520,opacity:interpolate(frame,[7*fps,7.5*fps],[0,1],clamp),
        scale:interpolate(frame,[7*fps,7.8*fps],[.7,1],clamp)}}>
        <FileCard output/>
        <div style={{marginTop:12,textAlign:"center",fontSize:24,color:"#37684b"}}>✓ Ready</div>
      </Interactive.Div>
      <Interactive.Div name="Conversion progress" premountFor={fps} style={{position:"absolute",left:250,top:914,width:580,
        opacity:interpolate(frame,[3.8*fps,4.1*fps],[0,1],clamp)}}>
        <div style={{height:7,background:"#cddedc",borderRadius:8,overflow:"hidden"}}>
          <div style={{height:"100%",width:`${interpolate(frame,[4*fps,7*fps],[0,100],clamp)}%`,background:"#537d6e"}}/>
        </div>
        <div style={{textAlign:"center",fontSize:28,marginTop:18}}>
          {t < 7 ? "Converting PNG → JPG" : "Export complete · Original kept"}
        </div>
      </Interactive.Div>
    </Interactive.Div>
    <Interactive.Div name="Closing card" premountFor={fps} style={{position:"absolute",inset:0,
      opacity:interpolate(frame,[9.2*fps,9.8*fps],[0,1],clamp),
      translate:`0px ${interpolate(frame,[9.2*fps,10*fps],[25,0],clamp)}px`}}>
      <CanvasImage src={staticFile("molla-icon.png")} premountFor={fps} style={{position:"absolute",top:355,left:440,width:200,height:200}}/>
      <div style={{position:"absolute",top:605,left:80,right:80,fontSize:70,letterSpacing:-3,textAlign:"center",lineHeight:1.15}}>
        Your files.<br/>A fresh form.
      </div>
      <div style={{position:"absolute",top:810,left:80,right:80,textAlign:"center",fontSize:36,color:"#58737c"}}>Images · Audio · Video</div>
    </Interactive.Div>
    <div style={{position:"absolute",bottom:40,left:80,right:80,display:"flex",justifyContent:"space-between",fontSize:19,color:"#6f858b"}}>
      <span>Made for Linux</span><span>Animated product demo · PNG → JPG</span>
    </div>
  </AbsoluteFill>;
};

export const RemotionRoot = () => <Composition id="MollaDemo" component={MollaDemo} width={1080} height={1080} fps={30} durationInFrames={360} />;
