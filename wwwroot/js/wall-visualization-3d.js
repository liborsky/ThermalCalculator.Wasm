/**
 * 3D Vizualizace tepelných vlastností zdi
 * Používá Three.js pro rendering 3D scény s teplotními gradienty
 */
class WallVisualization3D {
    constructor(containerId) {
        this.containerId = containerId;
        this.container = document.getElementById(containerId);
        
        // Three.js komponenty
        this.scene = null;
        this.camera = null;
        this.renderer = null;
        this.controls = null;
        
        // Vizualizační data
        this.wallData = null;
        this.config = {
            mode: 'temperature',
            showCondensationZones: true,
            showLabels: true,
            enableAnimation: false,
            animationSpeed: 1.0,
            colorScheme: 'blueRedScale'
        };
        
        // 3D objekty
        this.wallGroup = new THREE.Group();
        this.labelsGroup = new THREE.Group();
        this.condensationGroup = new THREE.Group();
        
        // Materiály a textury
        this.materials = new Map();
        this.gradientTextures = new Map();
        
        // Animace
        this.animationId = null;
        this.animationTime = 0;
        
        // Event listenery
        this.resizeHandler = () => this.onWindowResize();
        this.blazorRef = null;
    }

    /**
     * Inicializace 3D scény
     */
    async init() {
        try {
            await this.setupScene();
            await this.setupCamera();
            await this.setupRenderer();
            await this.setupControls();
            await this.setupLighting();
            
            this.scene.add(this.wallGroup);
            this.scene.add(this.labelsGroup);
            this.scene.add(this.condensationGroup);
            
            // Základní scéna je připravena - objekty budou přidány při aktualizaci dat
            
            
            // Event listenery
            window.addEventListener('resize', this.resizeHandler);
            
            this.animate();
            
            console.log('3D Vizualizace inicializována úspěšně');
            return true;
        } catch (error) {
            // Bezpečné logování chyby
            if (window.console && console.log) {
                console.log('Chyba při inicializaci 3D vizualizace:', error.message || error);
            }
            return false;
        }
    }

    /**
     * Nastavení základní scény
     */
    async setupScene() {
        this.scene = new THREE.Scene();
        this.scene.background = new THREE.Color(0xf5f5f5);
        
        // Mlha pro lepší vizuální efekt
        this.scene.fog = new THREE.Fog(0xf5f5f5, 100, 1000);
    }

    /**
     * Nastavení kamery
     */
    async setupCamera() {
        const aspect = this.container.clientWidth / this.container.clientHeight;
        this.camera = new THREE.PerspectiveCamera(60, aspect, 0.1, 2000);
        
        // Pozice kamery pro dobrý výhled na řez zdi
        this.camera.position.set(500, 300, 800);
        this.camera.lookAt(0, 0, 0);
    }

    /**
     * Nastavení rendereru
     */
    async setupRenderer() {
        this.renderer = new THREE.WebGLRenderer({ 
            antialias: true,
            alpha: true,
            preserveDrawingBuffer: true 
        });
        
        let width = this.container.clientWidth || 800;
        let height = this.container.clientHeight || 500;
        
        // Pokud je výška příliš malá, použij minimum
        if (height < 100) {
            height = 500;
            this.container.style.height = '500px';
        }
        
        this.renderer.setSize(width, height);
        this.renderer.setPixelRatio(Math.min(window.devicePixelRatio, 2));
        this.renderer.shadowMap.enabled = true;
        this.renderer.shadowMap.type = THREE.PCFSoftShadowMap;
        
        // Kompatibilita s různými verzemi Three.js
        if (THREE.sRGBEncoding) {
            this.renderer.outputEncoding = THREE.sRGBEncoding;
        }
        if (THREE.ACESFilmicToneMapping) {
            this.renderer.toneMapping = THREE.ACESFilmicToneMapping;
        }
        this.renderer.toneMappingExposure = 1.0;
        
        this.container.appendChild(this.renderer.domElement);
    }

    /**
     * Nastavení ovládání kamery
     */
    async setupControls() {
        // Zkusíme načíst OrbitControls, pokud existují
        if (typeof THREE !== 'undefined' && THREE.OrbitControls) {
            this.controls = new THREE.OrbitControls(this.camera, this.renderer.domElement);
            this.controls.enableDamping = true;
            this.controls.dampingFactor = 0.05;
            this.controls.screenSpacePanning = false;
            this.controls.minDistance = 100;
            this.controls.maxDistance = 2000;
            this.controls.maxPolarAngle = Math.PI;
            
            // Defaultní target na střed zdi
            this.controls.target.set(0, 0, 0);
            this.controls.update();
        } else {
            // Základní nastavení kamery bez ovládání
            this.camera.position.set(500, 300, 800);
            this.camera.lookAt(0, 0, 0);
        }
    }

    /**
     * Nastavení osvětlení
     */
    async setupLighting() {
        // Ambient světlo
        const ambientLight = new THREE.AmbientLight(0x404040, 0.4);
        this.scene.add(ambientLight);
        
        // Hlavní směrové světlo
        const directionalLight = new THREE.DirectionalLight(0xffffff, 0.8);
        directionalLight.position.set(100, 200, 100);
        directionalLight.castShadow = true;
        
        // Nastavení stínů
        directionalLight.shadow.mapSize.width = 2048;
        directionalLight.shadow.mapSize.height = 2048;
        directionalLight.shadow.camera.near = 0.5;
        directionalLight.shadow.camera.far = 500;
        directionalLight.shadow.camera.left = -200;
        directionalLight.shadow.camera.right = 200;
        directionalLight.shadow.camera.top = 200;
        directionalLight.shadow.camera.bottom = -200;
        
        this.scene.add(directionalLight);
        
        // Doplňkové světlo z druhé strany
        const fillLight = new THREE.DirectionalLight(0xffffff, 0.3);
        fillLight.position.set(-100, 100, -100);
        this.scene.add(fillLight);
        
        // Horní světlo
        const topLight = new THREE.DirectionalLight(0xffffff, 0.2);
        topLight.position.set(0, 300, 0);
        this.scene.add(topLight);
    }

    /**
     * Aktualizace dat zdi
     */
    updateWallData(wallData) {
        this.wallData = wallData;
        this.rebuildWall();
    }

    /**
     * Přestavění celé zdi
     */
    rebuildWall() {
        if (!this.wallData) return;
        
        // Vyčištění existujících objektů
        this.clearWall();
        
        // Vytvoření nových vrstev
        this.createWallLayers();
        
        // Aplikace aktuálního módu vizualizace
        this.applyVisualizationMode();
        
        // Aktualizace kondenzačních zón
        if (this.config.showCondensationZones) {
            this.createCondensationZones();
        }
        
        // Aktualizace popisků
        if (this.config.showLabels) {
            this.createLabels();
        }
        
        // Centrování kamery na zeď
        this.centerCameraOnWall();
    }

    /**
     * Vyčištění existujících objektů
     */
    clearWall() {
        this.wallGroup.clear();
        this.labelsGroup.clear();
        this.condensationGroup.clear();
        this.materials.clear();
        this.gradientTextures.clear();
    }

    /**
     * Vytvoření vrstev zdi
     */
    createWallLayers() {
        if (!this.wallData || !this.wallData.layers) return;
        
        const totalThickness = this.wallData.totalThickness;
        const wallHeight = this.wallData.wallHeight || 2800;
        const wallWidth = this.wallData.wallWidth || 1000;
        
        // Převod na Three.js jednotky (mm → jednotky scény)
        const scale = 0.1; // 1mm = 0.1 jednotky scény
        
        let currentZ = -totalThickness * scale / 2; // Začít od středu
        
        this.wallData.layers.forEach((layer, index) => {
            // Minimální tloušťka pro zobrazení tenkých folií (5mm místo skutečné tloušťky)
            const actualThickness = layer.thickness * scale;
            const displayThickness = Math.max(actualThickness, 0.5); // Minimálně 0.5 jednotky scény (5mm)
            const height = wallHeight * scale;
            const width = wallWidth * scale;
            
            // Geometrie vrstvy s upravenou tloušťkou pro zobrazení
            const geometry = new THREE.BoxGeometry(width, height, displayThickness);
            
            // Materiál podle módu vizualizace
            const material = this.createLayerMaterial(layer, index);
            this.materials.set(layer.materialName, material);
            
            // Mesh vrstvy
            const mesh = new THREE.Mesh(geometry, material);
            mesh.position.set(0, 0, currentZ + displayThickness / 2);
            mesh.castShadow = true;
            mesh.receiveShadow = true;
            
            // Metadata pro interakci
            mesh.userData = {
                layerIndex: index,
                layerData: layer,
                materialName: layer.materialName,
                thickness: layer.thickness, // Skutečná tloušťka pro výpočty
                displayThickness: displayThickness / scale // Zobrazovací tloušťka
            };
            
            this.wallGroup.add(mesh);
            
            // Posun pro další vrstvu - použít skutečnou tloušťku pro správné umístění
            currentZ += actualThickness;
        });
    }

    /**
     * Vytvoření materiálu pro vrstvu
     */
    createLayerMaterial(layer, index) {
        const props = layer.visualProperties;
        
        let material;
        
        if (this.config.mode === 'temperature') {
            // Teplotní gradient materiál
            material = this.createTemperatureGradientMaterial(layer);
        } else if (this.config.mode === 'materials') {
            // Standardní materiál podle typu
            material = new THREE.MeshLambertMaterial({
                color: new THREE.Color(props.baseColor),
                transparent: true,
                opacity: Math.max(0.9, props.opacity), // Minimální opacity 0.9 pro lepší čitelnost
                roughness: props.roughness || 0.5,
                metalness: props.metalness || 0.0
            });
        } else {
            // Výchozí materiál
            material = new THREE.MeshLambertMaterial({
                color: 0x888888,
                transparent: true,
                opacity: 0.9 // Zvýšeno z 0.8 na 0.9
            });
        }
        
        return material;
    }

    /**
     * Vytvoření materiálu s teplotním gradientem
     */
    createTemperatureGradientMaterial(layer) {
        const canvas = this.createTemperatureGradientTexture(layer);
        const texture = new THREE.CanvasTexture(canvas);
        texture.wrapS = THREE.RepeatWrapping;
        texture.wrapT = THREE.RepeatWrapping;
        
        this.gradientTextures.set(layer.materialName, texture);
        
        return new THREE.MeshLambertMaterial({
            map: texture,
            transparent: true,
            opacity: Math.max(0.95, layer.visualProperties.opacity || 0.8) // Teplotní gradienty s vyšší opacity
        });
    }

    /**
     * Vytvoření textury s teplotním gradientem
     */
    createTemperatureGradientTexture(layer) {
        const canvas = document.createElement('canvas');
        canvas.width = 256;
        canvas.height = 256;
        const ctx = canvas.getContext('2d');
        
        if (!this.wallData.temperatureData || !this.wallData.temperatureData.profiles) {
            // Fallback - uniformní barva
            ctx.fillStyle = layer.visualProperties.baseColor;
            ctx.fillRect(0, 0, canvas.width, canvas.height);
            return canvas;
        }
        
        // Gradient přes Z-osu (tloušťku vrstvy)
        const gradient = ctx.createLinearGradient(0, 0, 0, canvas.height);
        
        const tempData = this.wallData.temperatureData;
        const minTemp = tempData.minTemperature;
        const maxTemp = tempData.maxTemperature;
        
        // Najít teplotní body pro tuto vrstvu
        const layerProfiles = tempData.profiles.filter(p => 
            p.position >= layer.startPosition && p.position <= layer.endPosition
        );
        
        if (layerProfiles.length > 0) {
            layerProfiles.forEach((profile, index) => {
                const normalizedPos = index / (layerProfiles.length - 1);
                const color = this.temperatureToColor(profile.temperature, minTemp, maxTemp);
                gradient.addColorStop(normalizedPos, color);
            });
        } else {
            // Fallback - střední teplota
            const avgTemp = (minTemp + maxTemp) / 2;
            const color = this.temperatureToColor(avgTemp, minTemp, maxTemp);
            gradient.addColorStop(0, color);
            gradient.addColorStop(1, color);
        }
        
        ctx.fillStyle = gradient;
        ctx.fillRect(0, 0, canvas.width, canvas.height);
        
        return canvas;
    }

    /**
     * Převod teploty na barvu
     */
    temperatureToColor(temperature, minTemp, maxTemp) {
        if (maxTemp <= minTemp) return '#888888';
        
        const normalized = Math.max(0, Math.min(1, (temperature - minTemp) / (maxTemp - minTemp)));
        
        switch (this.config.colorScheme) {
            case 'blueRedScale':
                return this.interpolateBlueRed(normalized);
            case 'rainbow':
                return this.interpolateRainbow(normalized);
            case 'thermal':
                return this.interpolateThermal(normalized);
            case 'monochrome':
                return this.interpolateMonochrome(normalized);
            default:
                return this.interpolateBlueRed(normalized);
        }
    }

    /**
     * Interpolace modra-červená
     */
    interpolateBlueRed(t) {
        const r = Math.floor(255 * t);
        const g = Math.floor(255 * (1 - Math.abs(2 * t - 1)));
        const b = Math.floor(255 * (1 - t));
        
        return `rgb(${r}, ${g}, ${b})`;
    }

    /**
     * Interpolace duha spektrum
     */
    interpolateRainbow(t) {
        const hue = Math.floor(240 * (1 - t)); // 240° (modrá) → 0° (červená)
        return `hsl(${hue}, 100%, 50%)`;
    }

    /**
     * Interpolace tepelná kamera
     */
    interpolateThermal(t) {
        let r, g, b;
        
        if (t < 0.25) {
            r = 0;
            g = 0;
            b = Math.floor(255 * (4 * t));
        } else if (t < 0.5) {
            r = 0;
            g = Math.floor(255 * (4 * (t - 0.25)));
            b = 255;
        } else if (t < 0.75) {
            r = Math.floor(255 * (4 * (t - 0.5)));
            g = 255;
            b = Math.floor(255 * (1 - 4 * (t - 0.5)));
        } else {
            r = 255;
            g = Math.floor(255 * (1 - 4 * (t - 0.75)));
            b = 0;
        }
        
        return `rgb(${r}, ${g}, ${b})`;
    }

    /**
     * Interpolace černobílá
     */
    interpolateMonochrome(t) {
        const intensity = Math.floor(255 * t);
        return `rgb(${intensity}, ${intensity}, ${intensity})`;
    }

    /**
     * Vytvoření kondenzačních zón
     */
    createCondensationZones() {
        if (!this.wallData.condensationZones) return;
        
        const scale = 0.1;
        const wallHeight = (this.wallData.wallHeight || 2800) * scale;
        const wallWidth = (this.wallData.wallWidth || 1000) * scale;
        const totalThickness = this.wallData.totalThickness * scale;
        
        this.wallData.condensationZones.forEach(zone => {
            const zoneThickness = (zone.endDepth - zone.startDepth) * scale;
            const zonePosition = (zone.startDepth + zone.endDepth) / 2 * scale - totalThickness / 2;
            
            const geometry = new THREE.BoxGeometry(
                wallWidth * 1.02, // Mírně větší než zeď
                wallHeight * 1.02,
                zoneThickness
            );
            
            const material = new THREE.MeshBasicMaterial({
                color: new THREE.Color(zone.visualizationColor),
                transparent: true,
                opacity: Math.max(0.5, zone.intensityAlpha || 0.3), // Kondenzační zóny lépe viditelné
                side: THREE.DoubleSide
            });
            
            const mesh = new THREE.Mesh(geometry, material);
            mesh.position.set(0, 0, zonePosition);
            
            // Metadata
            mesh.userData = {
                type: 'condensationZone',
                zoneData: zone
            };
            
            this.condensationGroup.add(mesh);
        });
    }

    /**
     * Vytvoření popisků
     */
    createLabels() {
        if (!this.wallData.layers) return;
        
        // TODO: Implementace 3D textových popisků
        // Pro nyní přeskočeno, bude implementováno později s HTML overlay
    }

    /**
     * Aplikace vizualizačního módu
     */
    applyVisualizationMode() {
        switch (this.config.mode) {
            case 'temperature':
                this.applyTemperatureMode();
                break;
            case 'materials':
                this.applyMaterialsMode();
                break;
            case 'condensation':
                this.applyCondensationMode();
                break;
            case 'heatFlow':
                this.applyHeatFlowMode();
                break;
            case 'dimensional':
                this.applyDimensionalMode();
                break;
        }
    }

    /**
     * Aplikace teplotního módu
     */
    applyTemperatureMode() {
        this.wallGroup.children.forEach(mesh => {
            if (mesh.userData.layerData) {
                const layer = mesh.userData.layerData;
                mesh.material = this.createTemperatureGradientMaterial(layer);
            }
        });
    }

    /**
     * Aplikace materiálového módu
     */
    applyMaterialsMode() {
        this.wallGroup.children.forEach(mesh => {
            if (mesh.userData.layerData) {
                const layer = mesh.userData.layerData;
                const props = layer.visualProperties;
                
                mesh.material = new THREE.MeshLambertMaterial({
                    color: new THREE.Color(props.baseColor),
                    transparent: true,
                    opacity: Math.max(0.9, props.opacity) // Materiálový mód s vyšší opacity
                });
            }
        });
    }

    /**
     * Aplikace kondenzačního módu
     */
    applyCondensationMode() {
        // Zvýraznění vrstev s kondenzací
        this.wallGroup.children.forEach(mesh => {
            if (mesh.userData.layerData) {
                const layer = mesh.userData.layerData;
                const hasCondensation = layer.hasCondensation;
                
                mesh.material = new THREE.MeshLambertMaterial({
                    color: hasCondensation ? 0xff4444 : 0x888888,
                    transparent: true,
                    opacity: hasCondensation ? 0.95 : 0.5 // Zvýšeno pro lepší kontrast
                });
            }
        });
    }

    /**
     * Aplikace módu tepelného toku
     */
    applyHeatFlowMode() {
        // TODO: Implementace animace tepelného toku
        this.applyMaterialsMode(); // Fallback
    }

    /**
     * Aplikace rozměrového módu
     */
    applyDimensionalMode() {
        this.applyMaterialsMode(); // Základní materiály s rozměrovými informacemi
    }

    /**
     * Centrování kamery na zeď
     */
    centerCameraOnWall() {
        if (!this.wallData) return;
        
        const scale = 0.1;
        const totalThickness = this.wallData.totalThickness * scale;
        const wallHeight = (this.wallData.wallHeight || 2800) * scale;
        
        // Nastavení cíle ovládání na střed zdi
        this.controls.target.set(0, 0, 0);
        
        // Optimální pozice kamery
        const distance = Math.max(totalThickness * 3, wallHeight * 1.5, 500);
        this.camera.position.set(distance * 0.7, distance * 0.5, distance);
        this.camera.lookAt(0, 0, 0);
        
        this.controls.update();
    }

    /**
     * Změna konfigurace vizualizace
     */
    updateConfig(newConfig) {
        this.config = { ...this.config, ...newConfig };
        
        if (this.wallData) {
            this.rebuildWall();
        }
    }

    /**
     * Export snímku scény
     */
    exportImage(format = 'png', quality = 1.0) {
        return this.renderer.domElement.toDataURL(`image/${format}`, quality);
    }

    /**
     * Změna velikosti okna
     */
    onWindowResize() {
        if (!this.camera || !this.renderer || !this.container) return;
        
        const width = this.container.clientWidth;
        const height = this.container.clientHeight;
        
        this.camera.aspect = width / height;
        this.camera.updateProjectionMatrix();
        
        this.renderer.setSize(width, height);
    }

    /**
     * Animační smyčka
     */
    animate() {
        this.animationId = requestAnimationFrame(() => this.animate());
        
        // Animace tepelného toku (pokud je povolena)
        if (this.config.enableAnimation) {
            this.animationTime += 0.016 * this.config.animationSpeed;
            this.updateAnimation();
        }
        
        // Aktualizace ovládání
        if (this.controls) {
            this.controls.update();
        }
        
        // Rendering - FORCE RENDER
        if (this.renderer && this.scene && this.camera) {
            try {
                this.renderer.render(this.scene, this.camera);
            } catch (error) {
                console.error('Rendering error:', error);
            }
        } else {
            console.error('Missing components for rendering:', {
                renderer: !!this.renderer,
                scene: !!this.scene,
                camera: !!this.camera,
                container: !!this.container
            });
        }
    }

    /**
     * Aktualizace animace
     */
    updateAnimation() {
        // TODO: Implementace animačních efektů (tepelný tok, pulsování kondenzace)
    }

    /**
     * Nastavení Blazor reference pro callback
     */
    setBlazorReference(blazorRef) {
        this.blazorRef = blazorRef;
    }

    /**
     * Zničení vizualizace
     */
    dispose() {
        // Zastavení animace
        if (this.animationId) {
            cancelAnimationFrame(this.animationId);
            this.animationId = null;
        }
        
        // Odebrání event listenerů
        window.removeEventListener('resize', this.resizeHandler);
        
        // Vyčištění Three.js objektů
        this.clearWall();
        
        if (this.renderer) {
            this.renderer.dispose();
            if (this.container && this.renderer.domElement) {
                this.container.removeChild(this.renderer.domElement);
            }
        }
        
        // Vyčištění materiálů a textur
        this.materials.forEach(material => material.dispose());
        this.gradientTextures.forEach(texture => texture.dispose());
        
        this.materials.clear();
        this.gradientTextures.clear();
    }
}

// Globální instance pro přístup z Blazoru
window.wallVisualization3D = null;

// Globální funkce pro Blazor interop
window.initWallVisualization3D = async function(containerId) {
    try {
        // Kontrola podpory WebGL
        if (!window.WebGLRenderingContext) {
            if (window.console && console.log) {
                console.log('WebGL není podporován v tomto prohlížeči');
            }
            return false;
        }
        
        // Kontrola Three.js
        if (typeof THREE === 'undefined') {
            if (window.console && console.log) {
                console.log('Three.js knihovna není načtena');
                console.log('Dostupné globální objekty:', Object.keys(window).filter(k => k.includes('THREE') || k.includes('three')));
            }
            return false;
        }
        
        // Logování úspěšného načtení
        if (window.console && console.log) {
            console.log('Three.js úspěšně načten, verze:', THREE.REVISION);
        }
        
        // Zničení existující instance
        if (window.wallVisualization3D) {
            window.wallVisualization3D.dispose();
        }
        
        // Vytvoření nové instance
        window.wallVisualization3D = new WallVisualization3D(containerId);
        const success = await window.wallVisualization3D.init();
        
        return success;
    } catch (error) {
        // Bezpečné logování chyby
        if (window.console && console.log) {
            console.log('Chyba při inicializaci 3D vizualizace:', error.message || error);
        }
        return false;
    }
};

window.updateWallVisualization3D = function(wallDataJson) {
    if (!window.wallVisualization3D) {
        if (window.console && console.log) {
            console.log('3D Vizualizace není inicializována');
        }
        return false;
    }
    
    try {
        const wallData = JSON.parse(wallDataJson);
        window.wallVisualization3D.updateWallData(wallData);
        return true;
    } catch (error) {
        if (window.console && console.log) {
            console.log('Chyba při aktualizaci 3D vizualizace:', error.message || error);
        }
        return false;
    }
};

window.updateVisualization3DConfig = function(configJson) {
    if (!window.wallVisualization3D) {
        if (window.console && console.log) {
            console.log('3D Vizualizace není inicializována');
        }
        return false;
    }
    
    try {
        const config = JSON.parse(configJson);
        window.wallVisualization3D.updateConfig(config);
        return true;
    } catch (error) {
        if (window.console && console.log) {
            console.log('Chyba při aktualizaci konfigurace 3D vizualizace:', error.message || error);
        }
        return false;
    }
};

window.exportVisualization3DImage = function(format = 'png', quality = 1.0) {
    if (!window.wallVisualization3D) {
        if (window.console && console.log) {
            console.log('3D Vizualizace není inicializována');
        }
        return null;
    }
    
    try {
        return window.wallVisualization3D.exportImage(format, quality);
    } catch (error) {
        if (window.console && console.log) {
            console.log('Chyba při exportu obrázku 3D vizualizace:', error.message || error);
        }
        return null;
    }
};

window.disposeWallVisualization3D = function() {
    if (window.wallVisualization3D) {
        window.wallVisualization3D.dispose();
        window.wallVisualization3D = null;
        console.log('3D Vizualizace byla zničena');
    }
};